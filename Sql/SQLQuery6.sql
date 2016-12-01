use scamps;
select * from scamps.dbo.environment;

INSERT INTO users  ( username, firstname, lastname, email, password, authmode,  active) VALUES ('Test2','Bob','TestTest','bob@fake.org', 'Test', 1,1);
select * from users;

select username from users where username = 'Tester';
select * from users where username = 'tester';