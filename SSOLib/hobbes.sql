--SQL statements stored for external access
--read using Scamps.Tools.ReadBlock(full path to this file, block name
--blocks are identified in this file using "--[blockname]--" on a new line
--

-- 2 types of queries listed, the ones with ".tok" in the name use token replacement
-- other queries should do parameterized queries (i was having problems getting them to work, fix later)
--[find.user.tok]--
select USER_NAME from users where USER_NAME = '{0}';

--[find.User]--
select User_Name from users where User_Name = @nametofind

--[get.User.Record]-- 
select * from users where User_Name = @nametofind"

--[find.User.Tok]-- 
select User_Name from users where User_Name = '{0}';

--[get.Hash.]--
select hash, salt from users where User_Name = @nametofind

--[get.Hash.Tok]-- 
select * from users where User_Name = '{0}'

--[update.User.Hash.Tok] 
update users set hash = '{0}', salt= '{1} where User_Name = '{2}';

--[mark.user.tok]--
update {0} set delete = TRUE, where User_Name = '{1}';
