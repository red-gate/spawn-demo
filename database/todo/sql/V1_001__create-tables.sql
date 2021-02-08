CREATE TABLE todo_list
(
    userId VARCHAR(70),
    id BIGSERIAL PRIMARY KEY,
    createdAt TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() AT TIME ZONE 'utc'),
    task TEXT NOT NULL,
    done BOOLEAN NOT NULL
);

CREATE TABLE projects
(
    userId VARCHAR(70),
    id BIGSERIAL PRIMARY KEY,
    createdAt TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() AT TIME ZONE 'utc'),
    name VARCHAR(255)
);