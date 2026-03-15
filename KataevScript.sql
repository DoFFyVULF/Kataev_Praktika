\c postgres

CREATE ROLE app WITH LOGIN PASSWORD '123456789';
CREATE DATABASE kataev;

\c kataev postgres;

CREATE SCHEMA app AUTHORIZATION app;
ALTER ROLE app SET search_path = app, public;

ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT USAGE, SELECT ON SEQUENCES TO app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app GRANT EXECUTE ON FUNCTIONS TO app;

\c kataev app;
SET search_path TO app;
SET client_encoding TO 'UTF8';

CREATE TABLE app."KataevPartners" (
    "KataevId"                  serial          PRIMARY KEY,
    "KataevName"                varchar(255)    NOT NULL,
    "KataevPartnerType"         varchar(100),
    "KataevRating"              integer         NOT NULL DEFAULT 0,
    "KataevAddress"             varchar(500),
    "KataevINN"                 varchar(12),
    "KataevDirectorLastName"    varchar(100),
    "KataevDirectorFirstName"   varchar(100),
    "KataevDirectorMiddleName"  varchar(100),
    "KataevPhone"               varchar(50),
    "KataevEmail"               varchar(255)
);

CREATE TABLE app."KataevProducts" (
    "KataevId"      serial          PRIMARY KEY,
    "KataevName"    varchar(255)    NOT NULL,
    "KataevArticle" varchar(100),
    "KataevType"    varchar(100)
);

CREATE TABLE app."KataevSalesHistory" (
    "KataevId"          serial  PRIMARY KEY,
    "KataevPartnerId"   integer NOT NULL REFERENCES app."KataevPartners"("KataevId") ON DELETE CASCADE,
    "KataevProductId"   integer NOT NULL REFERENCES app."KataevProducts"("KataevId") ON DELETE CASCADE,
    "KataevQuantity"    integer NOT NULL CHECK ("KataevQuantity" > 0),
    "KataevSaleDate"    date    NOT NULL DEFAULT CURRENT_DATE
);

CREATE INDEX idx_kataev_saleshistory_partnerid  ON app."KataevSalesHistory" ("KataevPartnerId");
CREATE INDEX idx_kataev_saleshistory_productid  ON app."KataevSalesHistory" ("KataevProductId");
CREATE INDEX idx_kataev_saleshistory_saledate   ON app."KataevSalesHistory" ("KataevSaleDate");
CREATE INDEX idx_kataev_partners_inn            ON app."KataevPartners"     ("KataevINN");

CREATE OR REPLACE FUNCTION app.kataev_calculate_discount(p_partner_id INTEGER)
RETURNS INTEGER
LANGUAGE plpgsql AS $$
DECLARE
    v_total INTEGER;
    v_discount INTEGER;
BEGIN
    SELECT COALESCE(SUM("KataevQuantity"), 0)
    INTO v_total
    FROM app."KataevSalesHistory"
    WHERE "KataevPartnerId" = p_partner_id;

    IF v_total < 10000 THEN
        v_discount := 0;
    ELSIF v_total < 50000 THEN
        v_discount := 5;
    ELSIF v_total < 300000 THEN
        v_discount := 10;
    ELSE
        v_discount := 15;
    END IF;

    RETURN v_discount;
END;
$$;

CREATE OR REPLACE FUNCTION app.kataev_get_partner_sales(p_partner_id INTEGER)
RETURNS TABLE (
    partner_name    varchar,
    product_name    varchar,
    product_article varchar,
    quantity        integer,
    sale_date       date,
    discount_pct    integer
)
LANGUAGE sql AS $$
    SELECT
        p."KataevName",
        pr."KataevName",
        pr."KataevArticle",
        sh."KataevQuantity",
        sh."KataevSaleDate",
        app.kataev_calculate_discount(p."KataevId")
    FROM app."KataevSalesHistory" sh
    JOIN app."KataevPartners" p  ON p."KataevId"  = sh."KataevPartnerId"
    JOIN app."KataevProducts" pr ON pr."KataevId" = sh."KataevProductId"
    WHERE sh."KataevPartnerId" = p_partner_id
    ORDER BY sh."KataevSaleDate" DESC;
$$;

CREATE OR REPLACE PROCEDURE app.kataev_add_sale(
    p_partner_id INTEGER,
    p_product_id INTEGER,
    p_quantity   INTEGER,
    p_sale_date  DATE DEFAULT CURRENT_DATE
)
LANGUAGE plpgsql AS $$
BEGIN
    IF p_quantity <= 0 THEN
        RAISE EXCEPTION 'Количество должно быть больше 0, передано: %', p_quantity;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM app."KataevPartners" WHERE "KataevId" = p_partner_id) THEN
        RAISE EXCEPTION 'Партнёр с ID % не найден', p_partner_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM app."KataevProducts" WHERE "KataevId" = p_product_id) THEN
        RAISE EXCEPTION 'Продукт с ID % не найден', p_product_id;
    END IF;

    INSERT INTO app."KataevSalesHistory"
        ("KataevPartnerId", "KataevProductId", "KataevQuantity", "KataevSaleDate")
    VALUES
        (p_partner_id, p_product_id, p_quantity, p_sale_date);
END;
$$;

INSERT INTO app."KataevPartners"
    ("KataevName", "KataevPartnerType", "KataevRating", "KataevAddress", "KataevINN",
     "KataevDirectorLastName", "KataevDirectorFirstName", "KataevDirectorMiddleName",
     "KataevPhone", "KataevEmail")
VALUES
    ('ООО СтройМастер',   'ООО', 7, 'г. Пермь, ул. Ленина, 12',      '5902345678',
     'Иванов',   'Пётр',      'Сергеевич',   '+7(342)123-45-67', 'stroymaster@mail.ru'),
    ('АО ПолимерГрупп',   'АО',  5, 'г. Екатеринбург, ул. Мира, 45', '6612345678',
     'Смирнова', 'Анна',      'Владимировна', '+7(343)987-65-43', 'polymer@mail.ru'),
    ('ИП Козлов Д.И.',    'ИП',  3, 'г. Пермь, ул. Садовая, 7',      '590212345678',
     'Козлов',   'Дмитрий',   'Игоревич',    '+7(342)555-01-22', 'kozlov@mail.ru');

INSERT INTO app."KataevProducts"
    ("KataevName", "KataevArticle", "KataevType")
VALUES
    ('Ламинат 33 класс',      'LAM-33-001', 'Напольное покрытие'),
    ('Паркетная доска',       'PRK-OAK-02', 'Напольное покрытие'),
    ('Кварцвиниловая плитка', 'KVP-GRY-03', 'Напольное покрытие'),
    ('Клей для паркета',      'GLU-PRK-04', 'Расходный материал'),
    ('Подложка хвойная',      'SUB-HVJ-05', 'Расходный материал');

INSERT INTO app."KataevSalesHistory"
    ("KataevPartnerId", "KataevProductId", "KataevQuantity", "KataevSaleDate")
VALUES
    (1, 1, 12000, '2025-01-15'),
    (1, 2,  5000, '2025-02-10'),
    (1, 3, 38000, '2025-03-05'),
    (2, 1,  9500, '2025-01-20'),
    (2, 4,  3000, '2025-02-28'),
    (3, 5,  1500, '2025-03-12');

\dt app.*
\di app.*
\df app.*

SELECT * FROM app."KataevPartners";
SELECT * FROM app."KataevProducts";
SELECT * FROM app."KataevSalesHistory";

SELECT
    p."KataevId",
    p."KataevName",
    COALESCE(SUM(sh."KataevQuantity"), 0) AS total_quantity,
    app.kataev_calculate_discount(p."KataevId") AS discount_pct
FROM app."KataevPartners" p
LEFT JOIN app."KataevSalesHistory" sh ON sh."KataevPartnerId" = p."KataevId"
GROUP BY p."KataevId", p."KataevName"
ORDER BY p."KataevId";

SELECT * FROM app.kataev_get_partner_sales(1);
