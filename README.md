Forgive the messy and hard to read code i was learning C# while i wrote this

What it does:
Upon first Load of the program it creates an XML file in the torch server /config directory
each consecutive load will load the XML file into a list
When a new player joins it adds to the XML document their steamID and a timestamp as well as adding it to the list
every 10 seconds it checks player owned grids for a safezone block
if a safezone block is found it checks the grid owner
once the owner is found it checks the list for the ID and gets the timestamp if the current time is 7 days above the timestamp it deletes the safezone block

Commands:
!protection time - This shows the time left until they can no longer use safezones
!protection disable - this sets the timestamp to 0 disabling the use of the safezone (For the people that dont want to be tempted) REQUIRES confirmation
