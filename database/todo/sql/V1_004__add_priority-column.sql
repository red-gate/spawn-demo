ALTER TABLE todo_list
ADD COLUMN Priority INT;

UPDATE todo_list SET Priority = 0;