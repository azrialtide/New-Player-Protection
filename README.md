Forgive the messy and hard to read code i was learning C# while i wrote this

What it does:
1. Upon first Load of the program it creates an XML file in the torch server /config directory
2. Each consecutive load will load the XML file into a list
3. When a new player joins it adds to the XML document and list their steamID and a timestamp
4. Every 10 seconds it checks player owned grids for a safezone block
5. If a safezone block is found it checks the grid owner
6. Once the owner is found it checks the list for the ID and gets the timestamp
7. If the current time is 7 days above the timestamp it deletes the safezone block

Commands:

!protection time - This shows the time left until they can no longer use safezones

!protection disable - Sets the timestamp to 0 disabling the use of the safezone REQUIRES confirmation
