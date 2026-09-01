-- Schema del database githubexplorer
-- Nessun ORM/migrations: lo schema è gestito a mano (vincolo della traccia: no EF).
-- Questo file viene eseguito automaticamente dal container MySQL al PRIMO avvio
-- (montato in /docker-entrypoint-initdb.d). Prima bozza: aggiustabile.

CREATE TABLE IF NOT EXISTS favorites (
    id          INT           NOT NULL AUTO_INCREMENT,
    github_id   BIGINT        NOT NULL,                 -- id del repo su GitHub (per dedup)
    name        VARCHAR(255)  NOT NULL,                 -- es. "AspNetCore"
    full_name   VARCHAR(512)  NOT NULL,                 -- es. "dotnet/AspNetCore"
    owner       VARCHAR(255)  NOT NULL,                 -- autore/owner
    html_url    VARCHAR(512)  NOT NULL,                 -- link al repo
    description  TEXT         NULL,                     -- può essere null lato GitHub
    stars       INT           NOT NULL DEFAULT 0,
    note        TEXT          NULL,                     -- nota personale (bonus)
    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_favorites_github_id (github_id)       -- impedisce duplicati
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
