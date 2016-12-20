--SQL statements stored for external access
--read using Scamps.Tools.ReadBlock(full path to this file, block name
--blocks are identified in this file using "--[blockname]--" on a new line
--

-- 2 types of queries listed, the ones with ".tok" in the name use token replacement
-- other queries should do parameterized queries (i was having problems getting them to work, fix later)
--[find.user.tok]--
select username from users where username = '{0}'

--[find.User]--
select username from users where username = @nametofind

--[get.User.Record]-- 
select * from users where username = @nametofind"

--[find.User.Tok]-- 
select username from users where username = '{0}';

--[get.Hash.]--
select hash, salt from users where username = @nametofind

--[get.Hash.Tok]-- 
select * from users where username = '{0}'

--[update.User.Hash.Tok] 
update users set hash = '{0}', salt= '{1} where username = '{2}';

--[mark.user.tok]--
update {0} set delete = TRUE, where username = '{1}';

--[select.whitelist.endpoints]--
 select value from environment where name like '%Whitelist-endpoint'