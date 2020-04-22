# AdSyncDecrypt

See here for explanation: https://vbscrub.com/2020/01/14/azure-ad-connect-database-exploit-priv-esc/

Note: This program must be run while the AD Sync Bin folder is your “working directory”, or has been added to the PATH variable. An easy way to do this is simply navigate to the folder in Powershell or Command Prompt (i.e cd “C:\Program Files\Microsoft Azure AD Sync\Bin”), and then run the program by typing the full path to wherever you have stored it. You also need to make sure the mcrypt.dll (icluded in the ZIP file you download from the Release tab) is in the same directory the program is in. Failure to do either of these things will result in a Module Not Found error.
