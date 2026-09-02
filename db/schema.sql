CREATE DATABASE IF NOT EXISTS githubexplorer
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE githubexplorer;

CREATE TABLE IF NOT EXISTS favorites (
    id          INT           NOT NULL AUTO_INCREMENT,
    github_id   BIGINT        NOT NULL,
    name        VARCHAR(255)  NOT NULL,
    full_name   VARCHAR(512)  NOT NULL,
    owner       VARCHAR(255)  NOT NULL,
    html_url    VARCHAR(512)  NOT NULL,
    description TEXT          NULL,
    stars       INT           NOT NULL DEFAULT 0,
    note        TEXT          NULL,
    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_favorites_github_id (github_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'ghexp_user'@'%' IDENTIFIED BY 'ghexp_dev_pass';
GRANT ALL PRIVILEGES ON githubexplorer.* TO 'ghexp_user'@'%';
FLUSH PRIVILEGES;
