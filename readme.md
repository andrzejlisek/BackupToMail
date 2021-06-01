# BackupToMail overwiev

This application is a command\-line application, which allows to upload any large file to any e\-mail account by splitting to segments\. Unlike Peer2Mail file sharing technology, there are some differences comparing to ordinary P2M applications:


* BackupToMail is intended to keep own data \(like backup\) on the own accounts\.
* BackupToMail does not generate hash information, so it is not intended to share your files to untrusted people and there is not data lose risk by losing hash information\.
* BackupToMail does not support in creating e\-mail accounts \(some P2M application can automatically create free of charge accounts on some servers\)\.
* BackupToMail can support any account by POP3 or IMAP protocol to download file, and SMTP protocol to upload file, other protocols are not supported\.
* BackupToMail works in console, so you can use it through text terminal connection \(telnet or ssh\) and you can execute download or upload from other application\.
* The message structure and upload/download technology is simple and easy to reproduce\.
* Dummy file feature \(simulating file without physical disk file\), which can be used to test account abilities in large file uploading or downloading, especially transfer speed and limits\.

If you run application without parameters or with unsupported action parameter, the application will print short description of command line syntax\. The action parameter is the first parameter and there is not case\-sensitive\.

# Configuration

BackupToMail uses the **Config\.txt** file to get configuration\. This file is a textual file, which contains the **parameter=value** lines, which defines general settings and account configuration\. The lines, which are not contains the **=** sign are ignored\.

## General settings

The general settings in **Config\.txt** file are following:


* **ThreadsUpload** \- Number of simultaneous connections and threads used in uploading \(default **1**\)\.
* **ThreadsDownload** \- Number of simultaneous connections and threads used in downloading \(default **1**\)\.
* **UploadGroupChange** \- Number of upload errors, after which the group of sending accounts will be changed to the nest group \(default **5**\)\.
* **DownloadRetry** \- Number of retries to download the same message after download failure and reconnection \(default **5**\)\.
* **DefaultSegmentType** \- Segment type and segment upload order when segment type is not specified in upload command \(default **0**\)\.
* **DefaultSegmentSize** \- Segment size when segment size is not specified in upload command \(default **16777216** = 16MB\)\.
* **DefaultImageSize** \- Default image size \(width\) when image size is not specified in upload command \(default **4096**\)\.
* **RandomCacheStepBits** \- Number of bits to specify caching period in generating the dummy file contents \(default **25**, which means 32MB\)\.
* **LogFileTransfer** \- If set, the transfer information \(uploading and downloading segments\) will be recorded into the file\.
* **LogFileMessages** \- If set, the transfer information \(all printed messages\) will be recorded into the file\.
* **LogFileSummary** \- If set, the transfer summary \(parameters and result\) will be recorded into the file\.
* **NameSeparator** \- The character, which will used as sepatator of multiple file names and item names\. If not set, the transfer of multiple files once is not possible\. If the value is longer than one character, only the first character will be used\.

When parameter is not set or has incorrect value, the default value will be used\.

## Account configuration

The **Config\.txt** file can consists the packet of settings with **Mail** followed by number prefix, the first number is 0\. Such parameter defines one e\-mail account\.

The parameters for account 0 are following:


* **Mail0Address** \- E\-mail address used as delivery address when upload\.
* **Mail0Login** \- Account login name\.
* **Mail0Password** \- Account password\.
* **Mail0SmtpHost** \- SMTP host address\.
* **Mail0SmtpPort** \- SMTP port number\.
* **Mail0SmtpSsl \(0/1\)** \- SSL usage on SMTP connection\.
* **Mail0ImapHost** \- IMAP host address\.
* **Mail0ImapPort** \- IMAP port number\.
* **Mail0ImapSsl \(0/1\)** \- SSL usage on IMAP connection\.
* **Mail0Pop3Host** \- POP3 host address\.
* **Mail0Pop3Port** \- POP3 port number\.
* **Mail0Pop3Ssl \(0/1\)** \- SSL usage on POP3 connection\.
* **Mail0Pop3Use \(0/1\)** \- Use POP3 instead of IMAP for download\. In is recommended to use IMAP protocol, but in some accounts POP3 can be more reliable\.
* **Mail0SmtpConnect \(0/1\)** \- Connect to SMTP server of this account directly before sending mail and disconnect after mail sending\. It can solve problems, which exists while sending several messages during one connection or if there is problem with reaining SMTP connection\.
* **Mail0DeleteIdx \(0/1\)** \- Delete index after deleting message\. Some accounts may delete item from index immediatelly after marking a message to delete, the other accounts may not delete item from index even if IMAP protocol us used\. Usually, the item is not deleted immediatelly after deleting message, the index is valid until disconnect from the account\. You have to experimentally detect, which valued of the setting is appropriate for certain account\.

The parameters indicated as **0/1** can have only **0** \(false\) or **1** \(true\) value\. The default value \(if parameter is not provided\) is **0**\. The account 1 has the same parameters, but with **Mail1** prefix istead of **Mail0** prefix\. Configuration is loaded as iterated loop while **Mail\_Address** is not blank\.

The number of accounts will be detected by account address\. If you define account 0, account 1 and account 3, ommiting account 2 \(or define account 2 without address\), the BackupToMail will load configuration for account 0 and account 1 only\.

## Configuration checking

You can check configuration and test accounts using **CONFIG** parameter\. The command line parameters are following:


* **CONFIG word** \- Print configuration and connection test,
* **Account list** \- List of accounts to print configuration or connection test\.
* **Test mode** \- Connection text one of following:
  * **0** \- Print configuration without test\.
  * **1** \- Connection test and print full configuration\.
  * **2** \- Connection test and print test results only\.
* **Number of tries** \- The maximum number of attepts to connect to the same server before raising error message\. If not provided, there will be performed single try\.

### General configuration checking

If you provide only first parameter, the application will print general configuration only and number of configured e\-mail accounts\.

```
BackupToMail.exe CONFIG
```

### Account configuration checking and testing

You can provide the account numbers separated by comma \(without space separation\) as second parameter to print loaded configuration about specified account\. For print cofiguration for account 1, 2 and 4, you have to run this command:

```
BackupToMail.exe CONFIG 1,2,4
```

You can test SMTP, IMAP and POP3 connection while configuration printing:

```
BackupToMail.exe CONFIG 1,2,4 1
```

You can also test connection without configuration details to check if all accounts are available and see all connection failures at first glance:

```
BackupToMail.exe CONFIG 1,2,4 2
```

Some accounts raises connection error once per several connection\. You can provide maximum number of tries for each test connection\. If the conection was good, there will be printed **OK**, otherwise, there will be printed error message from last connection attempt\. For the following command, there will be performed maximum 5 tries for each server of each account before printing error message:

```
BackupToMail.exe CONFIG 1,2,4 1 5
```

Some accounts requires sign in within certain time period since last sign in \(see account terms of use for details\)\. The connection test signs in to every specified server and resets the inactivity time\. In all connection failure cases, there will be printed the error message returned from the server\.

# Map file

The main actions, such as uploading and downloading files creates or uses the map file, which must be provided to do such action\. This file can be used to:


* Get detailed information, which segment was downloaded or checked as good\.
* Continue action if application working or system working was broken during last action\.
* Download only missing segments from another mail account\.
* Upload only the segments, which are missing or bad\.

## Contents and interpretation

The map file is textual file and contains as many bytes as number of file segments\. If the file does not exist, it will be created\. The map file can consist of tharacter from the following set:


* Before action:
  * **0** \- The segment will be processed during this action\.
  * **1** \- The segment will not be processed, at the action begin, all **1** occurences will be replaced with **2** and treated as **2**\.
  * **2** \- The segment will not be processed, because it was processed during previous action using the same map file\.
* After action
  * **0** \- The segment was should to be processed, but not processed\.
  * **1** \- The segment was processed during action\.
  * **2** \- The segment was not processed, due to no necessary\.

In some cases, especially after download or cheching, the map file size may be less than number of file segments\. The map file works against the following rules:


* If map file not exists, it will be created while the first write to the file, the first read will not create the file and characters from not existing map file is treated as **0**\.
* If attemp of writing beyond end of file takes place, the file there will be extended by writing **0** character as many times, as necessary, to write required character\.
* If attemp of reading beyond end of file takes place, the file will not be modified and unread character is treated as **0**\.
* Every character other than \(**0**, **1**, **2**\) is treated as **0**\.
* Characters beyond last segment byte \(in case, where map file size is greater than number of file segments\) is ignored, but keeping the contents is not guaranteed during action\.

To simplify, actions will be described by supposing that the map file exists and consists of the same number of bytes, as the number of file segments\.

## Dummy map file

You can use map file without real map file by providing blank name as **""** or slash **"/"** as map file name\. Such map will be treated as regular map file filled in with **0** characters only, which forces to process all segments\.

## Map file information

Large map file is difficult to analyze manually\. You can count good and bad segments using the following parameters\.


* **MAP word** \- Print map file information
* **File and print mode** \- One of file and display modes:
  * **0** \- Data file or dummy file, print full information
  * **1** \- Digest file, print full information
  * **2** \- Data file or dummy file, print brief information
  * **3** \- Digest file, print brief information
* **Data file name or digest file name** \- File name, in **0** or **2** mode, you can use dummy file definition
* **Segment size** \- Optional parameter used in **0** or **2** mode\. If ommited, the default segment size will be used\. For digest file \(mode **1** or **3**\), the segment size will be read from digest file\.

In 2 and 3 modes, there will be printed the following information in one line per file in the following order


* Data file name or digest file name\.
* Slash character\.
* Map file name\.
* Four numbers of segments in this order:
  * Total\.
  * Good previously\.
  * Good\.
  * Bad\.

To display information about **file\.map** file related to **file\.zip**, you can run such command:

```
BackupToMail.exe MAP 0 "D:\file.zip" "D:\file.map"
```

The only important thing about data file is file size and segment size\. In the command above, the default segment size will be used\. You can use custom segment size to calculate number of segments\.

```
BackupToMail.exe MAP 0 "D:\file.zip" "D:\file.map" 1000000
```

You can use dummy file definition, if you know file size, in this example, the size of simulated file is 500000000 bytes:

```
BackupToMail.exe MAP 0 "*500000000,2,," "D:\file.map" 1000000
```

The file contents are not important, so you can use any valid dummy file definition with desired size\. The only important is the number of segments of data file, so the following command will give the same result as above command:

```
BackupToMail.exe MAP 0 "*500,2,," "D:\file.map" 1
```

If you have digest file named **file\.dig**, you can use id to read map file:

```
BackupToMail.exe MAP 1 "D:\file.dig" "D:\file.map"
```

In this case, the segment size and number of segments will be read from the digest file, even, if you provide custom segment size\.

If the **NameSeparator** in **Config\.txt** is set, you can print information about more than one file at once command, when you set multiple data/digest files and map files\. If you assume, that the **NameSeparator** character is **&#124;**, you can print information about three files by such command:

```
BackupToMail.exe MAP 0 "D:\file1.zip|D:\file2.zip|D:\file3.zip" "D:\file1.map|D:\file2.map|D:\file3.map"
```

The separator character can not be used in item or file name\. Otherwise, you have to change this character in **Config\.txt** and use it in command\. Every list should consist of the same items\. If not, the number of uploaded files will equal with the number of item of the shortest list\. The further items on other lists will be ignored\.

# Uploading file

To upload file, you have to access to at least one account with SMTP server and define destination accounts\. The destination account can be the same as the source account\. Before uploading file, it is highly recommended to:


* Turn off spam filter on all your destination accounts or configure spam filter bypass for messages sent from adresses of every your source account\.
* Turn off body modifiers such as signature footer when you want to upload as Base64 or PNG image in body, without attachment\.
* Turn off autoresponder on every destination account if such service is on\.
* Encrypt file or archive, the BackupToMail does not support encryption, there are many archive file formats, which supports encryption, such as ZIP, 7Z and RAR\.

## File name remarks

Some operating system allows to use the **\*** character \(asterisk\) in file name, but BackupToMail interprets file name starting with **\*** as dummy file \(described separately\)\. You can upload file, which name starts with **\*** by on of the ways:


* Rename file name\.
* Create symbolic link do the file, which name not starts with **\*** and use it instead of file\.
* Provide file name with another path to achieve path not starting with **\***\.

Examples of names, which will be interpreted as dummy file and will raise error:


* \*file\.zip
* \*docs/file\.zip

Examples of names, which will be interpreted as regular file and you do not have to rename file or create symbolic link:


* file\*\.zip
* f\*ile\.zip
* /home/user/docs/\*file\.zip
* \.\./\*file\.zip

## Minimum account configuration

To perform upload, for every source account, there are used the following parameters: **Mail\_Login**, **Mail\_Password**, **Mail\_SmtpHost**, **Mail\_SmtpPort**, **Mail\_SmtpSsl**, **Mail\_SmtpConnect**

The last four parameters are usually common for every account on the same server\.

For each destination account, there are used the one parameter only: **Mail\_Address**

Other parameters are not used for upload\.

## Performing upload

To upload file, you have to run BackupToMail with the following parameters, the first four parameters are required, the last three parameter is optional, but every optional parameter must be provided, if you want to provide a further parameter:


1. **UPLOAD word** \- Perform upload action\.
2. **Item name** \- Item name \(identifier\) used on account \(it not must be the same as file name\)\.
3. **Data file path and name** \- Path and name of file, which you want to upload\.
4. **Map file path and name** \- Path and name of file, which you want to upload\. You can use the blank name or **/** character as name to not use map file\.
5. **Source account list** \- List of source accounts, which will be used to send messages\. You can use the **\.\.** in account list to separate the account groups\.
6. **Destination account list** \- List of destination accounts, which will be used provide message reipts into **To** field\.
7. **Segment size** \- The size of on segment other than default\.
8. **Segment type** \- The segment type and upload segment order other than default \(you can not provide segment type without providing segment size\), using one of the numbers:
  * **0** \- Binary attachment, ascending segment order\.
  * **1** \- PNG image attachment, ascending segment order\.
  * **2** \- Base64 in plain text body, ascending segment order\.
  * **3** \- PNG image in HTML body, ascending segment order\.
  * **10** \- Binary attachment, descending segment order\.
  * **11** \- PNG image attachment, descending segment order\.
  * **12** \- Base64 in plain text body, descending segment order\.
  * **13** \- PNG image in HTML body, descending segment order\.
9. **Image width** \- The image width used, if segment type is **1** or **3** or **10** or **13** \(you can not provide image width without providing segment type\)\.

If item name, data file name or map file name contains spaces, you have to provide this parameter in quotation signs like "file name with spaces"\. The source and destination account list can not contain a spaces\. Below, there are some examples:

Upload **file\.zip** using **file\.map** as map file, save item named as **File** from accounts 1 and 2 to accounts 2 and 3, use default settings:

```
BackupToMail.exe UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3
```

Upload **file\.zip** using the same accounts with provide 1000 image width and 1000000 bytes segment length:

```
BackupToMail.exe UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3 1000000 1 1000
```

Upload **file\.zip** using the same accounts using reverse segment order as binary attachment and default segment length:

```
BackupToMail.exe UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3 0 10
```

Upload **file\.zip** using four accounts in two groups to store in account 0:

```
BackupToMail.exe UPLOAD File D:\docs\file.zip D:\docs\file.map 0,1,..,2,3 0
```

Upload **file\.zip** without map file, save item named as **File** from account 0 to account 0, use default settings:

```
BackupToMail.exe UPLOAD File D:\docs\file.zip / 0 0
```

Upload **file\.zip** without map file, save item named as **File** from account 0 to account 0, use default settings \- alternative way:

```
BackupToMail.exe UPLOAD File D:\docs\file.zip "" 0 0
```

Upload **file with spaces\.zip** using **file with spaces\.map** as map file, save item named as **File** as Base64 encoded in message body from account 0 to account 0\.

```
BackupToMail.exe UPLOAD File "D:\docs by user\file with spaces.zip" "D:\docs by user\file with spaces.map" 0 0 1000000 2
```

## Upload several files at once

If the **NameSeparator** in **Config\.txt** is set, you can upload more than one file at once command, when you set multiple item names, data files and map files\. If you assume, that the **NameSeparator** character is **&#124;**, you can upload three files by such command:

```
BackupToMail.exe UPLOAD "File1|File2|File3" "D:\file1.zip|D:\file2.zip|D:\file3.zip" "D:\file1.map|D:\file2.map|D:\file3.map" 0 0
```

The separator character can not be used in item or file name\. Otherwise, you have to change this character in **Config\.txt** and use it in command\. Every list should consist of the same items\. If not, the number of uploaded files will equal with the number of item of the shortest list\. The further items on other lists will be ignored\.

The files will be uploaded sequentially, so, there is not substantial difference between using one command to upload several files and using several commands to upload one file per one command\.

## Upload principle

BackupToMail will upload only this segments, which are provided to upload against map file\. To upload whole file, be sure, that provided map file not exists or consists of **0** characters only\.

Before upload, all **1** occurences in the map file will be replaced with **2**\.

The number of segments of whole file is calculated based on data file size and one segment size\. The upload loop is iterated on all segments with printing informaton, which segment will be uploaded\. If there are found the same number of segments to uploads as number of upload threads, the application will assign source account to every segment to upload and the segments will be uploaded simultaneously in separate threads \(the one attemp will take place\)\. The time of preparing and sending mails will be measured and after upload attemps all segments, there will be printed upload speed based on successfully uploaded segments\.

While every segment of the threads upload failed, the application will change source account assignment from the same group and repeat upload attemp\. If some of all segments was uploaded, there will be read only as number of the segments as number of threads subtracted with number of upload failed segments\. Every failed upload will cause upload attemp repeat \(with changing source account assignment from the same group\) until such segment will be successfully uploaded\.

The account group will be changed after certain number of failures in serie, which is specified as **UploadGroupChange** value \(if not specified, the default is 5\)\. If none of account is available due to reach sending limits or internet connection break, this uploading attemps will repeaded infinitely, until sending limits on account is reset in at least of one account and internet connection is recovered\. Grouping the sending account may reduce the upload transfer obstruction caused by transfer limit per account\. Such limit exists very often in the free of charge accounts and are specified as certain number of messages per one hour or one day\.

After iteration of last segment, there will be performed uploading all not uploaded segment by the same way as uploading segment during iteration through segments\. It is no possible, that this action is end before uploading all segments\. Eventually, you can break this action by killing the BackupToMail process, the uploaded segments already will be denoted as **1** character in map file\.

# Downloading or checking file

To download or check file, you have to access to at least one account with IMAP or POP3 server, which keeps uploaded file\. Both actions uses the same mechanism with slightly work differences\. During downloading or checking it is possible deletion of specified kind of messages\.

## File name remarks

Some operating system allows to use the **\*** character \(asterisk\) in file name, but BackupToMail interprets file name starting with **\*** as dummy file \(described separately\)\. You can upload file, which name starts with **\*** by on of the ways:


* Rename file name\.
* Create symbolic link do the file, which name not starts with **\*** and use it instead of file\.
* Provide file name with another path to achieve path not starting with **\***\.

Examples of names, which will be interpreted as dummy file and will raise error:


* \*file\.zip
* \*docs/file\.zip

Examples of names, which will be interpreted as regular file and you do not have to rename file or create symbolic link:


* file\*\.zip
* f\*ile\.zip
* /home/user/docs/\*file\.zip
* \.\./\*file\.zip

## Minimum account configuration

To perform download, for each account containing file segments, there are used the following parameters: **Mail\_Login**, **Mail\_Password**, **Mail\_Pop3Use**\.

Apart from parameters mentioned above, there are other parameters, which usage depends on **Mail\_Pop3Use** parameter\. The following parameters are usually common for every account on the same server\.


* For **Mail\_Pop3Use=0**, there are used this parameters: **Mail\_ImapHost**, **Mail\_ImapPort**, **Mail\_ImapSsl**\.
* For **Mail\_Pop3Use=1**, there are used this parameters: **Mail\_Pop3Host**, **Mail\_Pop3Port**, **Mail\_Pop3Ssl**\.
* When you perform deletion in any way, there also used the **Mail\_DeleteIdx** parameter\.

The other parameters may used in downloading\.

## Delete index test

Some accounts requires **Mail\_DeleteIdx** \(the **\_** character specifies the account number, for account 0, the name of the parameter is **Mail0DeleteIdx**\) set to **0**, while all other accounts requires set to **1**\. This value depends on POP3/IMAP implementation on this account\. Usually, all accounts on the same server has the same required value for **Mail\_DeleteIdx** parameter\.


* The **Mail\_DeleteIdx=0** means, that message deletion will not change the index until disconnection\. In the place of deleted message, there will be void item\. The iterator position and upper bound must not be changed\.
* The **Mail\_DeleteIdx=1** means, that after message deletion, the item will be removed from the index and the index length will be decreased\. The iterator position and upper bound must be decreased by number of deleted messages\.

You have to test to determine, which value of the parameter is valid for the specified account\.

The recommended way is described step by step below\.

### Step 1

This description assumes, that the account to be tested is assigned to account 0 in **Config\.txt** file\.

Set the **Mail0DeleteIdx **value to **0**\. This value means, that index item will not be removed with message deletion\.

### Step 2

If the account is empty, upload some test data, to get from 10 to 100 messages\. You can use the following command:

```
BackupToMail.exe UPLOAD "test" "*50000000,2,," "" 4 4 1000000
```

This command will generate 50 messages\.

### Step 3

Print the account contents by the following commands:

```
BackupToMail.exe DOWNLOAD "test" "" "" 4 1 0
```

You will get the segment order in the account\. Consider, that the segment order may not be the same as uploaded\.

### Step 4

Perform clearing account using the following command, obserwing the information printed to the screen:

```
BackupToMail.exe DOWNLOAD "test" "" "" 4 1 1,2,3,4,5,6
```

### Test result

If the account actually requires the same value od **Mail0DeleteIdx** as the value is set, in the **Step 4** you will get the segment list in the same order like in the **Step 3** and consider, that the **Mail0DeleteIdx** value is correct\. 

If the account actually requires **Mail0DeleteIdx=1** while the parameter is set to **0**, in the **Step 4**\. you will get the segment list containing odd segments \(the 1st, 3rd, 5th and so on\) and after the last segment there will be header download error\. The message number will be a half of message count in account\. Fo this example, this error will be after 25th message, while attempt to process the 26th message\. In this case, consider, that **Mail0DeleteIdx** must be set to **1** and change value in the **Config\.txt** file\.

If the account actually requires **Mail0DeleteIdx=0** while the parameter is set to **1**, in the **Step 4**, you wil get the 1st segment, but while attemping to process the 2nd segment \(which will be 1st after deletion\), ypu will get header download error\. BackupToMail will reconnect to POP3/IMAP and will again delete the first message and will get header download error\. In this case, consider, that **Mail0DeleteIdx** must be set to **0** and change value in the **Config\.txt** file\.

To be sure, after **Mail0DeleteIdx **value change, repeat the test from **Step 2** to **Step 4**\.

## Performing download/check

To download or check file, you have to run BackupToMail with the following parameters, the first five parameters are required, the last two parameter is optional, but every optional parameter must be provided, if you want to provide a further parameter:


1. **DOWNLOAD word** \- perform download or check action\.
2. **Item name** \- Item name \(identifier\) used on account \(it not must be the same as file name\)\.
3. **Data file path and name** \- Path and name of file, which you want to upload\.
4. **Map file path and name** \- Path and name of file, which you want to upload\. You can use the blank name or **/** character as name to not use map file\.
5. **Source account list with item index intervals** \- Account list separated by commas, but pair of numbers separated by two dots \(**\.\.**\) or one number and two dots is interpreted as index interval filter \(see examples\)\.
6. **Download or check mode** \- One of available modes and index browsing direction, which uses the same principle \(some of this modes implies header download only\), the mode and direction is a number from the following:
  * **0** or **10** \- Download data file \(default mode, which is used, if this parameter is not specified\)\.
  * **1** or **11** \- Check existence without body control\.
  * **2** or **12** \- Check existence with body control\.
  * **3** or **13** \- Check the header digest using data file\.
  * **4** or **14** \- Check the body contents using data file\.
  * **5** or **15** \- Download digest file\.
  * **6** or **16** \- Check the header digest using digest file\.
  * **7** or **17** \- Check the body contents using digest file\.
  * From **0** to **7** \- forward browsing direction\.
  * From **10** to **17** \- backward browsing direction\.
7. **Delete option list** \- List of values separated by commas, which indicates, which messages must be deleted \(additionaly with download/check action\):
  * **0** \- None\.
  * **1** \- Bad \- after certain number of attempts in a row\.
  * **2** \- Duplicate\.
  * **3** \- This file\.
  * **4** \- Other messages\.
  * **5** \- Other files\.
  * **6** \- Undownloadable messages \- after certain number of attempts in a row\.

Because the downloading or checking principle is browsing messages item by item \(information, which messages contains desired item, not exists\), BackupToMail can browse the message index in forward or backward dorection\. The browsing order depends on downoad od check mode as following:


* Between **0** and **7** \- forward browsing direction
* Between **10** and **17** \- backward browsing direction

The browsing order is not important in most cases\. The cases, in which browsing order affects the result or working time, are for example:


* In the account, there exists messages with the same subject, due to uploading two different files, which has the same number of segments using the same item name\.
* All messages containing desired item are rathet at the index beginning or ending, but you not specify message number range\.

If item name, data file name or map file name contains spaces, you have to provide this parameter in quotation signs like "file name with spaces"\.

There are some examples, if the order is not described, it means forward order:

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages in forward order:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages in backward order:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1 10
```

Download **File** and save as **file\.zip** without map file from account 1 with reading all messages:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip / 1
```

Download **File** and save as **file\.zip** without map file from account 1 with reading all messages \- alternative way:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip "" 1
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from the first to 50:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,..50
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to the last:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..50
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50, then account 3 with reading all messages:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..50,2
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages, then account 3 with reading all messages:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,2
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50, then account 3 with reading messages from 20 to 40:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..50,2,20..40
```

Download **File** and save as **file with spaces\.zip** using **file with spaces\.map** as map file from account 1 with reading all messages:

```
BackupToMail.exe DOWNLOAD File "D:\docs by user\file with spaces.zip" "D:\docs by user\file with spaces.map" 1
```

Check in forward order, if **File** item exists on account 1 with reading all messages, in this action data file name is not used:

```
BackupToMail.exe DOWNLOAD File dummy D:\docs\file.map 1 1
```

Check in backward order, if **File** item exists on account 1 with reading all messages, in this action data file name is not used:

```
BackupToMail.exe DOWNLOAD File dummy D:\docs\file.map 1 11
```

Check, if **File** item exists on account 1 with reading all messages, delete bad and duplicate messages of this item, in this action data file name is not used:

```
BackupToMail.exe DOWNLOAD File dummy D:\docs\file.map 1 1 1,2
```

Delete **File** item from account 1 and 2 in forward order:

```
BackupToMail.exe DOWNLOAD File dummy D:\docs\file.map 1,2 1 3
```

Delete **File** item from account 1 and 2 in backward order:

```
BackupToMail.exe DOWNLOAD File dummy D:\docs\file.map 1,2 11 3
```

Download and delete **File** item from account 1 and 2:

```
BackupToMail.exe DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,2 0 3
```

Clear account 1 and 2 \(the item name and file name are not important, any file will not be created and not tried to read in **1** or **11** mode\):

```
BackupToMail.exe DOWNLOAD File dummy / 1,2 1 3,4,5
```

## Download several files at once

If the **NameSeparator** in **Config\.txt** is set, you can download more than one file at once command, when you set multiple item names, data files and map files\. If you assume, that the **NameSeparator** character is **&#124;**, you can download three files by such command:

```
BackupToMail.exe DOWNLOAD "File1|File2|File3" "D:\file1.zip|D:\file2.zip|D:\file3.zip" "D:\file1.map|D:\file2.map|D:\file3.map" 0
```

The separator character can not be used in item or file name\. Otherwise, you have to change this character in **Config\.txt** and use it in command\. Every list should consist of the same items\. If not, the number of downloaded files will equal with the number of item of the shortest list\. The further items on other lists will be ignored\.

The file will be downloaded by browsing the account, so, if you want to download several files from the same account, using one command to download several files is faster, because the account will be browser once to get all files instead of several times, everytime for each file\.

## Download principle

BackupToMail will download only this segments, which are provided to download against map file\. To download whole file, be sure, that provided map file not exists or consists of **0** characters only\.

Before download, all **1** occurences in the map file will be replaced with **2**\.

If you provide more than one account, the file will be downloaded from the accounts sequentially, so some segments will be downloaded from account other than the first only when the segments does not exist on previously browsed accounts\. If all neccesary segments is downloaded, the next accounts will not be browsed\.

Within one account browsing, there will be analyzed all messages in the account\. The number of messages to browse can be limited by providing index interval described in performing command description\. You can provide the first index, the last index or both the first and last index\. It is usable, when you know approximetly the index interval, within there is file to download\.

Every browsed message information is printed and the subject is analyzed to determine, if the message seems to be contain a rewuested file\. The number of file segments is not known until there will be browsed one of the segments matching to requested item \(subject contains the digest of item name\)\. Such messages will not be downloaded immediately\. The downloading will be begin, if occurs one of the following events:


* Found as number of messages containing requested file parts do download as number of download threads\.
* After last message to download there are browsed as number of other messages as number of download threads\.
* After browsing the last message in the browsing iteration loop\.

The time during downloading, from creating download threads to saving to file each segment from this threads, is measused and there is encountered byten of successfully downloaded segment\. After threads end, application prints download speed\.

If internet connection lost or IMAP/POP3 server is temporally unavailable while header browsing, BackupToMail reconnects and decrements iteration index by 1 to repeat header browsing attemp of the same message\. The download is performed in separated threads and there will be done one attemp of download each message, which should be downloaded\. BackupToMail checks, that message index in every connection points to the same message\. If no, all connections will be reconnected like, in case of connection lost during message download and messade download attemp will be repeated\.

If file is downloaded without deletion options, the download process ends immediately after download last missing segment\. You can download the digest file \(mode **5**\) instead of data file \(mode **0**\), thi data segments for the digest file will be downloaded exactly by the same way as download data file\.

## Digest file

Fo any data file, you can generate the digest file, which consists of digest for each data file segment\. The first 32 characters of digest file designes the file size and segment size, each occupies 16 bytes\. The further bytes are the segment digests, each consists of 32 characters\.

To generate or check the digest file, you have provide the following parameters:


1. **DIGEST word** \- generate or check the digest file\.
2. **Mode** \- One of the following modes:
  * **0** \- Create the digest file from the data file\.
  * **1** \- Check the data file against the digest file\.
  * **2** \- Correct the data file size\.
  * **3** \- Correct the data file size and check the data file\.
3. **Data file name** \- The name of data file, which will used to create or check the digest file\.
4. **Map file name** \- The map file name\.
5. **Digest file name** \- The name of the digest file\.
6. **Segment size** \- The size of one data file segment\.

The first five parameters are required and the sixth parameter is optional\. If the segment size is not provided or incorrect, there will be used the default segment size\.

To create the digest **SomeArchive\.dig** of **SomeArchive\.zip** file using **SomeArchive\.map** map file and **1000000** segment size, you have to execute the following command:

```
BackupToMail.exe DIGEST 0 SomeArchive.zip SomeArchive.map SomeArchive.dig 1000000
```

There will be displayed progress of digest creation\. The map file will contains only **1** marks after creating whole digest file\.

To check the file **SomeArchive\.zip** against the **SomeArchive\.dig** file using **SomeArchive\.map** map file and **1000000** segment size, you have to execute the following command:

```
BackupToMail.exe DIGEST 1 SomeArchive.zip SomeArchive.map SomeArchive.dig 1000000
```

In the map file \(if map file name is given\), the good segments will be saved as **1** and other segment will be marked as **0**\.

You can do the same operation without map file:

```
BackupToMail.exe DIGEST 1 SomeArchive.zip / SomeArchive.dig 1000000
```

If the data file size may be incorrect, you have to execute the following command, which will corect the data file size to match the size stored in digest file \(this command will not check the data file contents against the digest file\):

```
BackupToMail.exe DIGEST 2 SomeArchive.zip SomeArchive.map SomeArchive.dig 1000000
```

There will be displayed the following information \(in mode **0**, **1** and **3**\):


* Match of the data size and segment size stored in the digest file\.
* Progress of creating or checking the digest\.
* Numbers of matched and mismatched segment digest\.

The digest file can be used as data file substitute to check the completeness and correctness of uploaded data without the original data file, especially, when the data file is very large\. You can download the digest file \(mode **5**\) instead of data file \(mode **0**\), the digests will be generated based on the data file segments\.

## Checking uploaded file

BackupToMail offers checking completeness and correctness of uploaded file by six ways:


* Check existence without body control\.
* Check existence with body control\.
* Check the header digest using data file\.
* Check the body contents using data file\.
* Check the header digest using digest file\.
* Check the body contents using digest file\.

The checking principle is the same as download principle and uses the same functionality\. There are difference compared to download: 


* Reading data file or no using data file instead of writing data file\.
* Displaying check results\.
* Process takes place to planned end, without breaking after checking all unchecked segments\.
* Some check modes takes place based on message headers instead of downloading whole message \(the **1** or **3** or **6** download/check mode\)\.

After checking, the transfer speed is displayes only for downloaded data bytes, while you perform the check mode, which requires download data\. The header data is not encountered to downloaded bytes\.

### Check existence without body control

There is the simpliest check type, it nos uses the data file name \(this parameter is ignored, although it must be provided\)\. BackupToMail researches headers of all messages, which matches to requested item name, like download action\. After find some file segment, application knowns the number of file segments\. This option can detect the following things: 


* Existence of some segments of requested file\.
* Existence of all or not all segments of file\.
* Duplicate segments\.
* Equality of number of segments in header of each existing segment \(the first found segment determines the correct number\)\.
* Equality of nominal segment size in header of each existing segment \(the first found segment determines the correct size\)\.

Thich checks does not download segments and is also useful to deletion messages\.

### Check existence with body control

The check type is similar to **Check existence without body control**, but additionally downloads segment\. This can detect the same things as above and additionally the following things: 


* The message contains readable segment bytes and is useful to download segment\.
* Compares real segment digest to digest provided in header\.

The check mode is very similar to download, the only difference is no saving the downloaded data\.

### Check the header digest

This mode reads data from provided local data file or digest name and generates the digest based on the file for every correct message, without browsing body or attachment\. The generated digest is compared with digest from header and if the digest differs, this messages is treated as bad\. This mode does not download segments from messages, it can detect: 


* Corrupted and incorrect messages\.
* Difference between local file contents and message contents by assuming that message content digest equals to digest saved in message header\.

Check the body contents like **Check the header digest**, this mode also reads the local data file or digest file, but downloads segment from body \(attachment or text\) and compares the downloaded segment with the same segment from local data file or generates the digest of the downloaded segment and compares with the digest from the digest file\. If differs, this message is bad\. 

## Deleting messages

During download or checking uploaded file, there is possible message deletion\. Because downloading or checking file requires browsing all messages \(or some messages within index interval\), there is possible to delete other messages, than messages related to requested file\. If you download with deletion options, the browsing will not be broken after downloading all segments\.

By default, no message is deleted during downloading or checking\. There are several modes of deletion, which can be combined:


* **Bad** \- Messages detected as bad during downloading or checking\.
* **Duplicate** \- Duplicate messages with existing segments\.
* **This file** \- Messages related to requested file\.
* **Other messages** \- Messages, which seems to not generated by BackupToMail\.
* **Other files** \- Messages related to other files than requested file\.

If you want to delete all existing messages from account, you have to delete map file \(if exists\) and run download or check any of file \(requested file may not exisits\) with deletion **This file**, **Other messages** and **Other files** options\.

Note: If you provide several accounts, which has the same segments, the same segments in the account other as first will be treated as duplicates\. It is recommended to run with deletion only on one account at a time\.

# Reed\-Solomon code

Some accounts are not as reliable as user thinks and sometimes loses some messages, statistically from 0 to 10 per thousand, depending on account provider\. So, if you upload larger file, which consists of about 1000 segments or more, you sooner or later may experience losing few segments due to some mysterious reason\. You can prevent from losing data by creating additional code file\. The file size determines, how may missing segments can be recovered\.

## Code generation and recovery principle

The code file is generated using Reed\-Solomon code and can be used to recover missing segments\. You can decide about size of the code as number of data segments\. There are possible two recovery scenarios:


* You do not know, which segments are missing or corrupted\. The code allows recovery up to half of the code size\. For example, if code consists of 6 segments or 7 segments, you can recover up to 3 corrupted segments, which will be found automatically\.
* You know, which segment are missing and you use this information during recovery process\. The code allows recovery as number of segments as code size\. For example, if code consists of 6 segments, you can recover up to 6 segments, if code consists of 7 segments, you can recover up to 7 segments\.

The code file is additional file and must be treated as other regular file\. You can upload it on the same account as another file or store this file locally only\.

The original Reed\-Solomon code uses the serie of n\-bit values, which determines the maximum number of segments \(total data file and code file\)\. One segment usually consists of from 1MB to 100MB of data, depending on message size limit\.

Each segment consists of certain bits \(segment size in bytes multiplied by 8\) and can be splitted into values\. For example, if segment consists of 50MB \(52428800 bytes\), you can interpret this as follwing, for example:


* 52428800 8\-bit values
* 41943040 10\-bit values
* 26214400 16\-bit values
* 20971520 20\-bit values

The number of value bits must be a integer divisor of segment size multiplied by 8\. For example, you can not split 52428800 bytes into 24\-bit values, because 24 is not divisor of 52428800\*8=419430400\.

The Reed\-Solomon code is based on the Galois finite fields, the code generation needs one of the primitive polynomials used to generate specified Galois field\. The primitive polynomial can be writtern as number\.

Below, there is presented features for each supported value size in bits\.

| Value bits | Power of 2 | Maximum number of segments | Default polynomial | Values:Bytes |
| --- | --- | --- | --- | --- |
| 2 | 4 | 3 | 7 | 8:2 = 4:1 |
| 3 | 8 | 7 | 11 | 8:3 |
| 4 | 16 | 15 | 19 | 8:4 = 4:2 = 2:1 |
| 5 | 32 | 31 | 37 | 8:5 |
| 6 | 64 | 63 | 67 | 8:6 = 4:3 |
| 7 | 128 | 127 | 131 | 8:7 |
| 8 | 256 | 255 | 285 | 8:8 = 4:4 = 1:1 |
| 9 | 512 | 511 | 529 | 8:9 |
| 10 | 1024 | 1023 | 1033 | 8:10 = 4:5 |
| 11 | 2048 | 2047 | 2053 | 8:11 |
| 12 | 4096 | 4095 | 4179 | 8:12 = 4:6 = 2:3 |
| 13 | 8192 | 8191 | 8219 | 8:13 |
| 14 | 16384 | 16383 | 16427 | 8:14 = 4:7 |
| 15 | 32768 | 32767 | 32771 | 8:15 |
| 16 | 65536 | 65535 | 65581 | 8:16 = 4:8 = 2:4 = 1:2 |
| 17 | 131072 | 131071 | 131081 | 8:17 |
| 18 | 262144 | 262143 | 262183 | 8:18 = 4:9 |
| 19 | 524288 | 524287 | 524327 | 8:19 |
| 20 | 1048576 | 1048575 | 1048585 | 8:20 = 4:10 = 2:5 |
| 21 | 2097152 | 2097151 | 2097157 | 8:21 |
| 22 | 4194304 | 4194303 | 4194307 | 8:22 = 4:11 |
| 23 | 8388608 | 8388607 | 8388641 | 8:23 |
| 24 | 16777216 | 16777215 | 16777243 | 8:24 = 4:12 = 2:6 = 1:3 |
| 25 | 33554432 | 33554431 | 33554441 | 8:25 |
| 26 | 67108864 | 67108863 | 67108935 | 8:26 = 4:13 |
| 27 | 134217728 | 134217727 | 134217767 | 8:27 |
| 28 | 268435456 | 268435455 | 268435465 | 8:28 = 4:14 = 2:7 |
| 29 | 536870912 | 536870911 | 536870917 | 8:29 |
| 30 | 1073741824 | 1073741823 | 1073741907 | 8:30 = 4:15 |

## Performing RS\-code operations

You can perform Reed\-Solomon code related operations by the following command:


1. **RSCODE word** \- Perform RS\-code related operation\.
2. **Mode** \- One of operation modes:
  * **0** \- Create code file\.
  * **1** \- Recover files automatically \- do not modify files\.
  * **2** \- Recover files based on the maps \- do not modify files\.
  * **3** \- Recover files automatically \- modify files according maps\.
  * **4** \- Recover files based on the maps \- modify files according maps\.
  * **5** \- Recover files automatically \- modify files regardless maps\.
  * **6** \- Recover files based on the maps \- modify files regardless maps\.
  * **7** \- Resize files to specified size in bytes\.
  * **8** \- Resize files to specified size in segments\.
  * **9** \- Simulate incomplete download\.
3. **Data file** \- Data file name\.
4. **Data map** \- Map file for data file\.
5. **Code file** \- Code file name\.
6. **Code map** \- Map file for code file\.
7. **Code segments** \- Number of code segments, used in mode **0** only, not affects in other modes\.
8. **Segment size** \- Segment size, if **0** or omitted, there will be used the default segment size\.
9. **Polynomial number** \- Value size or primitive polynomial number:
  * **0 or omitted** \- Use as small value size as possible with default primitive polynomial\.
  * **Power of 2 \(4, 8, 16, 32\.\.\.\)** \- Force specified value size with default primitive polynomial\.
  * **Every other number** \- Force specified primitive polynomial, not every polynomial is actually primitive polynomial\.

Every Galois field has certain set of primitive polynomials, which matches the following formula, the `a` is array of values:

```
a[n]*x^n + a[n-1]*x^(n-1) + a[n-2]*x^(n-2) + ... + a[2]*x^2 + a[1]*x + a[0]
```

The `n` value is the number of bits, the every `a[k]` can equal to **0** or **1**, so the `a` array can be represented as number generated using the following formula:

```
a[n]*2^n + a[n-1]*2^(n-1) + a[n-2]*2^(n-2) + ... + a[2]*4 + a[1]*2 + a[0]*1
```

Searching for primitive polynomial is complex and is not describet here\. It is recommended to use default polynomial\. If you use number, which represents non\-primitive polynomial, the application will freeze or generate useless code, if you run code generation \(mode 0\)\.

## Generating code file

To protect file from accidentally losing some segments, you have to create code file, which consists of desired number of segments\. For data and code file, there will be created maps, which consists of 1 for every segment\.

For example, to create **Archive\.rsc** code file consisting of 10 segments for **Archive\.zip** data file and create map files \(**Archive\.map** for **Archive\.zip** and **Archive\.rsm** for **Archive\.rsc**\), you can use the following command:

```
BackupToMail.exe RSCODE 0 Archive.zip Archive.map Archive.rsc Archive.rsm 10
```

In this case, application will automatically detect and print used value size depending on number of all segments \(totally data file and code file\) and one segment size\. There will be used default segment size\. The previous maps contents does not affect the code generation process\.

If you want to use 10\-bit values, you have to use 2 powered to 10 \(equals **1024**\) as number of primitive polynomial:

```
BackupToMail.exe RSCODE 0 Archive.zip Archive.map Archive.rsc Archive.rsm 10 0 1024
```

In this case, there will be used the default polynomial number for 10\-bit values\.

You can also force specified polynomial number:

```
BackupToMail.exe RSCODE 0 Archive.zip Archive.map Archive.rsc Archive.rsm 10 0 4179
```

The **4179** polynomial implies using 12\-bit values, because **4179** is greater than 4096 \(2 powered to 12\) and less than 8192 \(2 powered to 13\)\. Important: If you use the number, which represents non\-primitive polynomial, the application will freeze or will generate useless code\.

You can also use another than default segment size, for example 1MB segments:

```
BackupToMail.exe RSCODE 0 Archive.zip Archive.map Archive.rsc Archive.rsm 10 1048576
```

Actually, you can use another segment size than segment size, which will be used to upload file\. For example, you can upload file using 50MB segments and generatoe code using 100MB segments\. This may help using less value size\.

## Checking and recovering data file

If you have the data file and code file, you can check, if data file is correct and try to repair data file \(recovery original content\) using code file\.

You have to use the same parameters \(number of code segments, primitive polynomial number\), which was used to create the code file\.

There are two possible recovery modes:


* **Recover files automatically** \- The application will detect itself, which segments are corrupted and recovery them, if possible\. This mode works, when there are lost or missing half of the number of code segments, for exaple, if code file consists of 10 or 11 segments, you can recovery up to 5 segments while all other segments survives\. Actually, if some segments are corrupted partially \(for example, code was generated using other segment size than segment size used to upload\), the corrupted segments will be calculated individually for every value\. Thus, in some cases, you can recovery more than half of code segment number if some segments are corrupted partially\. The maps contents does not affect the recovery process, they affects only saving the process result\. For example, if data file consists of 100 segments and code file consists of 10 segments, you can recover all data and code in such cases:
  * At least 95 segments of data are correct and whole code file survives\.
  * 98 segments of data are correct and 3 segments of code file are correct\.
  * Whole data file survives and and at least 5 segments of code file are correct\.
  * 90 segments of data file are correct, the first half of 5 segments are corrupted, the second half of other 5 segments are corrupted and whole code file survives\.
* **Recover files based on the maps** \- The maps specified, which segments are missing or corrupted \(fully or partially\)\. In this mode, you can recovery as number of segments as number of code segments\. For example, if code consists of 10 segments, you can recovery up to 10 segments while all other segments survives, if code consists of 11 segments, you can recovery up to 11 segments while all other segments survives\. For example, if data file consists of 100 segments and code file consists of 10 segments, you can recover all data and code in such cases:
  * At least 90 segments of data are correct, all 10 corrupted segments are marked in map file as **0** and whole code file survives\.
  * At least 95 segments of data are correct, all 5 corrupted segments are marked in map file for data as **0**, 5 segments of code file are correct and the remaining 5 segments are marked in map for code file as **0**\.

Apart from the recovery modes, there are three possible save modes:


* **Do not modify files** \- The recovery proces will not modify files, it will display recovery result only\. This mode can be used to check, if you provide appropriate parameters or to ensure if file is correct\.
* **Modify files according maps** \- The application will perform recovery process, but there will be saved modifications inside this segments, which are marked as **0** in map file\.
* **Modify files regardless maps** \- The application can freely modify data file and code file during recovery\.

The recovery and save modes are compines as mode from **1** to **6** as following:


* **1** \- Recover files automatically \- do not modify files\.
* **2** \- Recover files based on the maps \- do not modify files\.
* **3** \- Recover files automatically \- modify files according maps\.
* **4** \- Recover files based on the maps \- modify files according maps\.
* **5** \- Recover files automatically \- modify files regardless maps\.
* **6** \- Recover files based on the maps \- modify files regardless maps\.

In every mode, the map files will not be modified\. It is not possible, which segments was actually incorrect, especially in automatically modes\. The result will give some informations which can be interpreted as series of values:


* **Total values per segment** \- Number of values per segment depending on segment size and value size\.
* **Values correct already** \- Values of all segments in serie seems be correct, the code values matches to data values\.
* **Recovered values in data only** \- Some serie values in data file was incorrect and fully recovered, all values in code file was correct already\.
* **Recovered values in code only** \- Some serie values in code file was incorrect and fully recovered, all values in data file was correct already\.
* **Recovered values in both data and code** \- Some serie values in both files was incorrect and fully recovered\.
* **Unrecoverable incorrect values** \- Too many serie values are incorrect and there is not possible to correct\. There also not possible to detect, which values in serie are correct or incorrect already\. If uncorrectable incorrect values exists, it means that there is not possible to fully recovery data file, in some cases, the file may be recovered partially\.

Depending on save mode, none, some or all modified segments will be saved\. The result is splitted to data and code file, thich contains following information:


* **Total** \- Total number of file segments\.
* **Modified and saved** \- Number of segments, which was partially or fully recovered and saved to data od code file\.
* **Modified and not saved** \- Number of segments, which was partially or fully recovered, but not saved due to selected save mode and map contents\.
* **Not modified** \- Number of segments, which was not modified due to not necessary or not possibility to recover\.

In most cases, there will be printed general recovery result depending on above number results\.

For example, to check, if file **Archive\.zip** data is correct against to **Archive\.rsc** code file, you can perform mode **1** without map files:

```
BackupToMail.exe RSCODE 1 Archive.zip / Archive.rsc /
```

If you want to recovery missing or corrupted segments without maps, you can use the **5** mode, if the code was created using 16\-bit values with default polynomial, you have to provide **65536** as polynomial \(2 powered to 16\):

```
BackupToMail.exe RSCODE 5 Archive.zip / Archive.rsc / 0 0 65536
```

If the code was created using 1MB segment size \(other than default\) and was used the **65581** polynomial, which was other than default, you have to provide these values:

```
BackupToMail.exe RSCODE 5 Archive.zip / Archive.rsc / 0 1048576 65581
```

If you have the **Archive\.map** map file to the **Archive\.zip** data file and you have the **Archive\.rsm** map file to the **Archive\.rsc** code file, you can recovery more missing segments based on the map file, and only segments marked as missing may be modified:

```
BackupToMail.exe RSCODE 4 Archive.zip Archive.map Archive.rsc Archive.rsm
```

## Code file and digest file

Assume, that you:


* Have the data file **Archive\.zip**, which is corrupted or incomplete\.
* Have the code file **Archive\.rsc**, which is correct and complete\.
* Do not have map file for both files\.
* Recovery data file in automatic mode was not succeeded\.
* Suspects, that recovery can be possible in based on the maps mode\.
* Have the digest **Archive\.dig** file\.

In this case, you have to generate the **Archive\.map** file by checking the data file against the digest file, if the file size is incorrect, the size will be corrected:

```
BackupToMail.exe DIGEST 3 Archive.zip Archive.map Archive.dig
```

This operaton will detect, which segments of **Archive\.zip** are correct and the file size will be corrected if is incorrect\. You will also get the the **Archive\.map** file, in which correct segments will be marked as **1** and incorret segments will be marked as **0**\.

For **Archive\.rsc** file, you also have to generate the map file\. Because you do not have the digest file, but you are sure, that the code file is correct, you need the map, which all segments of code file are marked as **1**\. To create the **Archive\.rsm** map file, you can use the digest file generation:

```
BackupToMail.exe DIGEST 0 Archive.rsc Archive.rsm /
```

The digest file will not be generated, but the **Archive\.rsm** map file will be generated\.

Now, you have the **Archive\.map** map file for **Achive\.zip** data file and **Archive\.rsm** map file for **Archive\.rsc** code file\. Then, you can perform recovery operation using map files:

```
BackupToMail.exe RSCODE 4 Archive.zip Archive.map Archive.rsc Archive.rsm
```

Now, if **Archive\.zip** file recovery was possible, you will get the recovered data file\. Check the printed recovery result\.

For this scenario, you can run the batch or script file, which contains the following commands:

```
BackupToMail.exe DIGEST 3 Archive.zip Archive.map Archive.dig
BackupToMail.exe DIGEST 0 Archive.rsc Archive.rsm /
BackupToMail.exe RSCODE 4 Archive.zip Archive.map Archive.rsc Archive.rsm
```

If you have the digest file for both data and code files \(**Archive\.dig** for **Archive\.zip** and **Archive\.rsd** for **Archive\.rsc**\), you can use both digest files to recovery original file sizes and maps:

```
BackupToMail.exe DIGEST 3 Archive.zip Archive.map Archive.dig
BackupToMail.exe DIGEST 3 Archive.rsc Archive.rsm Archive.rsd
BackupToMail.exe RSCODE 4 Archive.zip Archive.map Archive.rsc Archive.rsm
```

## Resizing data and code files

If you not downloaded at least one last segments from account, the file size will be incorrect\. The recovery using Reed\-Solomon code needs the correct file of both files\. If you do not have the digest file, you have to know the original file size and correct the size manually\.

To correct size, you can use the **RSCODE** function with mode **7**, providing correct size instead of map file names\. The map files are not needed in this action, the segment size does not affect this function working\.

Assume, that the correct size of **Archive\.zip** is 512354254 and correct size of **Archive\.rsc** is 5000000\.

You can resize both file at a time using the following command:

```
BackupToMail.exe RSCODE 7 Archive.zip 512354254 Archive.rsc 5000000
```

If you want to resize single file at a time, provide blank file name and size for secod file, because **RSCODE** requires minimum 6 parameters:

```
BackupToMail.exe RSCODE 7 Archive.zip 512354254 "" 0
```

So, if you download both data and code files, you can correct the file sizes and repair them automatically, when number of missing segments are less than number of code segments:

```
BackupToMail.exe RSCODE 7 Archive.zip 512354254 Archive.rsc 5000000
BackupToMail.exe RSCODE 3 Archive.zip / Archive.rsc /
```

You can resize files providing number of segments instead of size in bytes using mode **8**\. Many file types allows to be slightly larger than original file size, while the end of file is padded with zeros\. If the defailt segment size is 1000000, you can perform the following command:

```
BackupToMail.exe RSCODE 8 Archive.zip 513 Archive.rsc 5
```

The code file will have original size, but the data file will be slightly oversized\. You also provide segment size instead of using default segment size:

```
BackupToMail.exe RSCODE 8 Archive.zip 513 Archive.rsc 5 0 1000000
```

## Simulating incomplete download

You can clear some segments in data file and code file to simulate file incomplete download for test purposes\. You can simulate such case using **RSCODE** with mode **9**\.

You have to manually edit the map file to select, which segments will be missing, the **0** means the missing segment, the **1** or **2** simulates surviving segments\.

The number of segments parameter has another meaning in mode 8\. There are possible values:


* **0** \- Do not resize files\.
* **1** \- Resize files like download process is finished, but not all segments was exists\.
* **2** \- Resize files like download process was broken \(for example, due to power blackout\)\.

The map files will not be modified\.

To simulate incomplete download for **Archive\.zip** using **Archive\.map** and for **Archive\.rsc** using **Archive\.rsm**, run the following command:

```
BackupToMail.exe RSCODE 9 Archive.zip Archive.map Archive.rsc Archive.rsm 1
```

If you want to process single file at a time, provide blank file name and size for second file, because **RSCODE** requires minimum 6 parameters:

```
BackupToMail.exe RSCODE 9 Archive.zip Archive.map "" "" 1
```

Like every other mode, this mode uses default segment size\. You can use other segment size providing it to command\. For example, if you want to use the segment size 1000000 and simulate broken download process, you can do this by following command for **Archive\.zip** file and **Archive\.map** map:

```
BackupToMail.exe RSCODE 9 Archive.zip Archive.map "" "" 2 1000000
```

# Reuploading missing segments

In some cases, there are possible situation, in such some of parts are missing\. Such cases can occur due to sending errors, limits or sometimes due to account errors\. In such case, you have to reupload missing segments\. BackupToMail does not have such function, but filling can be achieved using upload and download function\.

For example, assume that, there is file named **Archive**, which exists on accounts **0** and **1**, but on both accounts some segments are missing, every segment is available from at least one account\. The repairing/filling steps are described below\.

## Detect missing segments

At the first step, you have to get information, which segments are missing, if any segment are missing\. To do this, you have to use download function to detect, which segment exists\. There are some variants of usage depending on having the file on the disk\. If map file with given name exists, remove it\.

If you do not have the file, you can use the mode 1 or 2 \(given file name does not matter, because it will not be read or written\):

```
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive.map 0 1
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive.map 0 2
```

If you have the original file \(saved as **Archive\.zip**\), you can use the mode 3 or 4:

```
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive.map 0 4
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive.map 0 4
```

If you have the digest file \(saved as **Archive\.dig**\), you can use the mode 6 or 7:

```
BackupToMail.exe DOWNLOAD Archive Archive.dig Archive.map 0 7
BackupToMail.exe DOWNLOAD Archive Archive.dig Archive.map 0 7
```

The generated Archive\.map file will contain information, which segments of Archive exists on account 0\.

## Download missing segments from another account

If you have original data file on the disk, you can ship this step\. If not, and you have the same file on the another account, for example in account **1**, you have to download missing segment of the file\. Downloading whole data file is not necessary to reupload missing segments\.

The map file will be modified during downloading, so you have copy the map file \(the copy command depends on your operating system\):

```
copy Archive.map Archive_copy.map
cp Archive.map Archive_copy.map
```

Then, you can download missing segments using the copied map file:

```
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive_copy.map 1 0
```

If account 1 contains all missing segments, at the result you will get information, that you have all segments of the file, no missing segments\. This information is generated by analyzing map file\. The ral file will probably seem be corrupted, but this file contains only the downloaded segments, not whole data file\. If account 1 does not contain all segments, which are missing on account **0**, but you have the same file on account **2**, you can repeat download proedure on account **2**:

```
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive_copy.map 2 0
```

The segments downloaded from account 1 will not affected, the map file has information, which segments are have downloaded\.

If you download all missing segments, you can remove the **Archive\_copy\.map** file\.

## Recover missing segments using code file

You can use the code file to recovery missing data segments, if you meet all following conditions:


* You do not have any copy of the the original data file in any storage\.
* You can not download all missing segments from another account\.
* You have the code file created by **RSCODE** function\.
* For both data and code file, you meets one of the following conditions:
  * You know the correct size of the file\.
  * You have the digest file\.

To recovery missing segments, at the first, you have to remove the map file if exists and download the data file:

```
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive.map 0 0
```

Next, if you do not have the code file on the disk, but you have stored this file on the account \(for example, as ArchCode item\), you have to download this file also:

```
BackupToMail.exe DOWNLOAD ArchCode Archive.rsc Archive.rsm 0 0
```

If you have the digest file for data, you have to automatically correct the data file size, the map file will not be needed:

```
BackupToMail.exe DIGEST 2 Archive.zip / Archive.dig
```

If you do not have the digest file, you can manually correct the data file size \(assume, that the data file size is 1000000000\):

```
BackupToMail.exe RSCODE 7 Archive.zip 1000000000 "" 0
```

If the code file is alco incomplete and the file size may be incorrect, you have to correct code file size by the similar way\.

If you are sure, that size of both files are correct, you can try to recovery missing segments\.

If number of missing segments \(total of data and code file\) are less than half of number of code file segments, you can try to recovery in automatic mode without using the maps:

```
BackupToMail.exe RSCODE 5 Archive.zip / Archive.rsc /
```

If the missing segments are more, but less or equal than number of code file segments, you have to ensure, that you have the maps for both files\. If you do not have the map file for code file, because you have the file already, you have to generate the map consisting of all segments marked as **1**, use **DIGEST** function to do this, the digest file will not necessary, so it will not be created\.

```
BackupToMail.exe DIGEST 0 Archive.rsc Archive.rsm /
```

Then you have to use the map files to recovery original data file using following command:

```
BackupToMail.exe RSCODE 4 Archive.zip Archive.map Archive.rsc Archive.rsm
```

If both files was successfully recovered, you can reupload missing segments of both files, as described below\.

## Reupload missing segments

The last step is upload missing segments on the account 0\. you have to use the map file, which was generated at the first step:

```
BackupToMail.exe UPLOAD Archive Archive.zip Archive.map 0 0
```

There is not matter, which account you will use to upload, the destination account is important\. If you have many segments to upload, you can use multiple accounts \(for example accounts **0** and **1**\) to reduce upload obstruction\.

```
BackupToMail.exe UPLOAD Archive Archive.zip Archive.map 0,1 0
```

You will get the information, that all segments are uploaded, because the map file will contains all segments markered as transfered\. You can remove the **Archive\.map** and **Archive\.zip** files at the moment, the files will not needed longer\.

You can check if really all segments egists using on of download/checking functions such as:

```
BackupToMail.exe DOWNLOAD Archive Archive.zip Archive.map 0 2
```

# Message structure

While uploading file, BackupToMail creates serie of messages \(one email per file segment\), which has specified subject and contains a file segment data\.

## Subject specification

The message subject contains the **X** character exactly 7 times, which splits the subject to 8 parts\. The first \(0\) and the last \(7\) part are not used and are blank in sending messages\.

The 6 parts from 1 to 6 can contain only digits and letters from **A** to **F**\. The parts are following:


1. Digest of item name \(result of MD5 function\)\.
2. Number of this segment decreased by 1, represented by hex number, the first segment is 0\.
3. Number of all segments decreased by 1, represented by hex number\.
4. Size of this segment \(can differ from nominal segment size in last segment\) decreased by 1, represented by hex number\.
5. Nominal segment size decreased by 1, represented by hex number\.
6. Digest of segment raw content \(result of MD5 function\)\.

The mail can store file segment data inside body or as attachment\. There are supported four segment types:


* Binary attachment\.
* PNG image attachment\.
* Base64 in plain text body\.
* PNG image resource used in HTML body\.
* PNG image embedded in HTML body\.

## Binary attachment specification

There is the simplest segment type\. Such message consists **data\.bin** attachment file, which contains raw segment data\. To avoid errors, the message body is not blank, and consists of **Attachment** word as plain text\.

## PNG image attachment specification

This type is simmilar to binary attachment\. The attachment is the image in PNG format, which encodes data as pixel color\. The PNG format uses lossless compression and such message can imitate the message with picture/photography\. The image is not linked in message body\. The image width is specified arbitrary, the image height results from segment size and image width\.

## Base64 in plain text body specification

The message of this type does not contain an attachment\. The segment data is encoded as Base64 and there is in plain text body\.

## PNG image resource used in HTML body specification

The message is similar to PNG image attachment, but the attachment is not a regular attached fie, instead of this, there is a resource used in HTML body, which contains the image **cid** link\. Some e\-mail application will not display the attachment as attached file\.

## PNG image embedded in HTML body specification

The message is similar to PNG image attachment, but the message does not contain an attachment\. The message consists of HTML body, where the image is embedded in HTML content\. Embedding image in HTML code complies HTML specification, but some e\-mail applications may not display image\.

# Dummy file

For the test purposes, you can use the dummy file, which is not real disk file, but it acts as same as real disk file in exception of that this file is read\-only and attempt to save to this file does not raise error\. It can be use to test transfer or store very large files without generating the real file on the disk\. The content of the file is computed by generators, in most cases the content are similar to pseudo\-random values\. To use dummy file, input dummy file parameters in place of the data file name\. The map file name cannot be replaced by dummy file\. The file name definition consists of the **\*** sign and has a serie of parameters separated by commas\.

The byte values can be generated sequentially only\. There is a reason, why the generator uses cache, which stores generator state periodically during generating values of contents\. Using the cache, generator does not have to generate values from beginning to generate file segment other than the first segment\. In the gereal settings, there is the **RandomCacheStepBits** parameter, which defines the caching period by number of bits\. If you set 25 \(default value\), the generator state will be store between every 33554432 \(32M\) values\. It is recommended to set the bits to get the value between quarter segment size and double segment size but the recommendation applying is not mandatory\. 

Using the dummy file, you can test some features without managing large files, such as:


* Upload/download speed and limit\.
* Transfer obstruction possibility due to account limitations\.
* Reliability of large fire storeage\.

Examples of upload 1GB dummy file and check if the file is complete and correctly uploaded:

```
BackupToMail.exe UPLOAD "TestItem" "*1073741824,0,4,2,10,125,6" "TestItem.txt" 0 0
BackupToMail.exe DOWNLOAD "TestItem" "*1073741824,0,4,2,10,125,6" "TestItem.txt" 0 4 0
```

## Linear congruential and Fibonacci generator

The two generators are very fast and simple, but the length of period and value distribution strongly depends on parameters\. The parameters followed by **\*** means as following for linear congruential and fibonacci generators:


1. **File size** \- file size in bytes\.
2. **Generator type** \- Pseudo random number generator type:
  * **0** \- Linear congruential
  * **1** \- Fibonacci
3. **Number of bits** \- there is a number of least significant bits of state value, which are used to generate byte value:
  * **1** \- use one least significant bit, use 8 values to generate one byte\.
  * **2** \- use two least significant bits, use 4 values to generate one byte\.
  * **4** \- use least significant nibble, use 2 values to generate one byte consisting of two nibbles\.
  * **8** \- use whole value modulo by 256 as one byte\.
4. **A constant** \- vaue used in generator\.
5. **B constant** \- vaue used in generator\.
6. **M constant** \- vaue used in generator\.
7. **Initial vector** \- the vector values are the further parameters:
  * Linear congruential: Exactly one value\.
  * Fibonacci: At least one value, for exaple if vector consists of 3 values, the definition has totally 9 parameters including 7 parameters for generator

For example, to create whole dummy file having 1000 bytes length, when **number of bits** is specified to 2, there will be generated 4000 values and sequence of foru values will be used to generate one byte\.

## Linear congruential generator

There is very simple generator\. The sequence period substantially depends on the A, B and M values\. Initial vector has always one value and the value is the initial state \(S0\)\. The next value will be generated using the following formula:

```
S[n] = (A * S[n-1] + B) mod M
```

Example for 1MB dummy file A=2, B=10, M=125, S=6 using 4 bits of number: `*1048576,0,4,2,10,125,6`

## Fibonacci generator

The generator derives from Fibonacci sequence\. Initial vector must have as number of values as the greather value from \{A, B\} set\. The next state value is calculated using the following formula:

```
S[n] = (S[n-A] + S[n-B]) mod M
```

The first value in the vector will be forgotten, but the last value will be saved nest to the formerly last value in the vector, so the vector size does not change during calculating values, but the vector stores always last values\.

Example for 1MB dummy file A=3, B=1, M=17, S=\(7,16,5\) using 4 bits of number: `*1048576,1,4,3,1,17,7,16,5`

## Digest generator

The digest generator has the following features comparing to linear congruential and Fibonacci generators:


* Slower and more complex
* Very long period
* More uniform byte value distribution

For digest generator, the parameters are as following:


1. **File size** \- file size in bytes\.
2. **Generator type** \- Pseudo random number generator type:
  * **2** \- Digest
3. **Data prefix **\- the hexadecimal representation of prefix data, it can be blank
4. **Data suffix** \- the hexadecimal representation of suffix data, it can be blank

The generator uses the MD5 digest function to generate next 16 bytes\. The argument of the first digest \(used for the first 16 bytes\) is generated by concatenation of the prefix and suffix\. The every next digest argument is generated by concatenation of prefix, previous digest and suffix\.

The prefix and suffix must be blank or consist of even number of hexadecimal digits, the letter case is not important\. If you want to use blank prefix or suffix, do not put any characters as prefix or suffix\. The single digest generation iteration is used to generate 16 bytes of file\.

### Digest generation examples

The examples uses hexadecimal strings to representate byte strings\.

Example for 1MB dummy file without prefix and suffix: `*1048576,2,,`


* 1st 16 bytes: MD5\(`<blank>`\) = `D41D8CD98F00B204E9800998ECF8427E`
* 2nd 16 bytes: MD5\(`D41D8CD98F00B204E9800998ECF8427E`\) = `59ADB24EF3CDBE0297F05B395827453F`
* 3rd 16 bytes: MD5\(`59ADB24EF3CDBE0297F05B395827453F`\) = `8B8154F03B75F58A6C702235BF643629`

Example for 1MB dummy file with prefix only: `*1048576,2,BAADF00D,`


* 1st 16 bytes: MD5\(`BAADF00D`\) = `A7E0F8AC46398A7876D1E40DD52C2AAB`
* 2nd 16 bytes: MD5\(`BAADF00DA7E0F8AC46398A7876D1E40DD52C2AAB`\) = `372210219737BC34361A8E365596FB20`
* 3rd 16 bytes: MD5\(`BAADF00D372210219737BC34361A8E365596FB20`\) = `76A4201329BDBE905B29D8062B39E6D9`

Example for 1MB dummy file with suffix only: `*1048576,2,,DEADCAFE`


* 1st 16 bytes: MD5\(`DEADCAFE`\) = `FD8A7358D0EE3819B94DFEC2C7BFE5DA`
* 2nd 16 bytes: MD5\(`FD8A7358D0EE3819B94DFEC2C7BFE5DADEADCAFE`\) = `EB413475E6C8E1CD808B6FD6046299E5`
* 3rd 16 bytes: MD5\(`EB413475E6C8E1CD808B6FD6046299E5DEADCAFE`\) = `B4F09A81CC6549E346CDA9D298888D31`

Example for 1MB dummy file with prefix and suffix: `*1048576,2,BAADF00D,DEADCAFE`


* 1st 16 bytes: MD5\(`BAADF00DDEADCAFE`\) = `C21A01947540F250518FA75EBF8A93D7`
* 2nd 16 bytes: MD5\(`BAADF00DC21A01947540F250518FA75EBF8A93D7DEADCAFE`\) = `10FE55B78559E42F4E9C0122F8EFEFDB`
* 3rd 16 bytes: MD5\(`BAADF00D10FE55B78559E42F4E9C0122F8EFEFDBDEADCAFE`\) = `C4FA8C98A8F79383675F80570D095E0F`

## \.NET internal generator

This generator uses pseudo random number generator implemented in \.NET/Mono library, which is not officially described and may vary between environments \(\.NET/Mono version or operating system\)\. It uses one parameter, which is called seed\. On the same environment, you will get the same sequence everytime using desired seed\. This generator does not use cache, because there is not possible to get or set generator internal state, so is not recommended to use as dummy file while uploading or dwnloading test, it is recommended to generate disk file and use it to test\. Unlike the generators mentioned above, the number of generated byte is internally stored and generator works as following:


* If the starting byte is next to the last generated byte or no bytes was generated, the generator will just generate the next bytes, this case occurs everytime, when you create disk file using dummy file generator\.
* If the starting byte is further than the next to the last generated byte, the generator will generate the same number of bytes as the distance from the last byte and first desired byte without storing, followed be, there will be generated more bytes to use as dummy file fragment\.
* If the starting byte is nearer than the next to the last generated byte, the generator will be reinitialized with the same seed and one of two above cases will occur\.

For \.NET internal generator, the parameters are as following:


1. **File size** \- file size in bytes\.
2. **Generator type** \- Pseudo random number generator type:
  * **3** \- \.NET internal
3. **Generator seed** \- This parameter is optional:
  * If exists: The seed number for generator \(from 0 to 2147483647\)\.
  * If not exists: The cryptographic generator will be used instead of standard generator\.

If the last parameter is ommited, the another algorithm will be used, which has the following features comparing to standard algorithm:


* Higher quality of randomness\.
* Does not use any parameters\.
* Generated sequence can not be repeated, so this generator is not usable to use dummy file on the fly, you can only create disk file and use it\.

Example for 1MB dummy file using standard algorithm: `*1048576,3,1234`

Example for 1MB dummy file using cryptographic algorithm: `*1048576,3`

## Create disk file

You can create real disk file, which has content the same as dummy file\. To do this, use the FILE command with following parameters:


* **FILE word** \- create file based on dummy file definition\.
* **Dummy file definition** \- the dummy file definition described above\.
* **File name** \- Real disk file name\.
* **Segment size** \- segment size used to display file creation progress\. If ommited or set as **0**, there will be used default segment size\.
* **File stats mode** \- One of the file statistics modes:
  * **0 \- No statistics** \- do not create statistics\.
  * **1 \- Simplified distribution table** \- print statistics as 16x16 table to look over the distribution at a first glance\. If value count exceedes 9999 \(four\-digit number\), all values will be divided by any power of 10 to achieve all values less than 10000\.
  * **2 \- Value list with zeros** \- print count of each value including zeros\.
  * **3 \- Value list without zeros** \- print count of each value excluding zeros\.
* **Period stats mode** \- One of the period statistics modes:
  * **0 \- No statistics** \- do not create statistics, the period will not be searched\.
  * **1 \- Simplified distribution table** \- print statistics as 16x16 table to look over the distribution at a first glance\. If value count exceedes 9999 \(four\-digit number\), all values will be divided by any power of 10 to achieve all values less than 10000\.
  * **2 \- Value list with zeros** \- print count of each value including zeros\.
  * **3 \- Value list without zeros** \- print count of each value excluding zeros\.

File creation example using segment size and without statistics and period searching:

```
BackupToMail.exe FILE "*500000000,1,8,3,1,257,7,16,5" File.bin 10000000
```

## Period and statistics

Both described generators generates bytes in period, which in many cases can be short, depending on input parameters \(dummy file definition\)\. You can use file creating to get statistics\. 

The period search may take a long of time depending on file size\. The progress is displayed by **period** and **occurence** values\. The algorithm tests if file contains the period of given size, begins from 1 and ends either if period found or if tested period size reach the file size\. Within testing one period length value, program checks all occurences if the maches the first occurence\. If any occurence does not match the first occurence, the further occurences will not be tested The progress will be displayed every 1 second using timer\.

Example to create file with display simple table with file distribution and period calculation with display period distribution using detailed list without zeros:

```
BackupToMail.exe FILE "*500000000,1,8,3,1,257,7,16,5" File.bin 0 1 2
```

# Batch operations

The following operations displays the operation parameters and waits for the confirmation by user:


* **UPLOAD** \- Uploading the file\.
* **DOWNLOAD** \- Downloading or cheching the file\.
* **DIGEST** \- Generating digest file or cheching data file against the digest file\.
* **RSCODE** \- Generating Reed\-Solomon code or recovering data file\.
* **FILE** \- Generating the data file based on dummy file parameters\.

If you execute one of the operations mentioned above, there will be displayed the question **Do you want to continue \(Yes/No\)**\. If you write one of the following answers \(letter case is not important\), there will be interpreted as **Yes**: **1**, **T**, **TRUE**, **Y**, **YES**\. Other answer will be interpreted as **No**\.

You can ommit the confirmation if you add the BATCH word to operation word ass following:


* **UPLOADBATCH** or **BATCHUPLOAD**
* **DOWNLOADBATCH** or **BATCHDOWNLOAD**
* **DIGESTBATCH** or **BATCHDIGEST**
* **RSCODEBATCH** or **BATCHRSCODE**
* **FILEBATCH** or **BATCHFILE**

This approach is very usable, when you want to run the BackupToMail from the script or batch file many times \(for example, to upload or download many files\)\.

The remaining operations \(**MAP** and **CONFIG**\) does not require confirmation anyway, because there are the following features:


* The header information before operation is not printed\.
* The general purpose are only check or test something only and print result about it\.
* The operations does not generate or modify any file or anything in any accont\.
* In most cases, these operations performing time is very short\.

# Log files

The upload and download operations can be logged to two log files\. To enable this feature, set the **LogFileTransfer** or **LogFileMessages** parameters to valid file name in **Config\.txt** configuration file\. If you set both parameters, the value must be different\.

In the log files, BackupToMail will log every upload or download action\. There will be saved the following informations:


1. Before action \- type of action and action informations such as file names and mail accounts\.
2. During action \- amount of uploaded or downloaded data during time \(transfer log file\) and all printed messages \(messages log file\)\.
3. After action \- information abount success, transfer time, average transfer rate\.

Every line is prefixed by current date and time\. It is recomended to open log file using spreadsheet software\.

The log file is not open always during working\. Application opens log file, adds the informatin, after this closes the log file\. If the BackupToMail will be broken suddenly \(for example, due to accidentally process killing\), the file will contain the last logged contents\.

## Transfer information

The detailed action progress informations will not stored into log file\. Instead of, there will be stored only summary of transfered data after every ending all transfer threads\. This information will have a table form and you should import/paste this text into any spreadsheet application\. The information will not generated, when you perform any checking or deleting file without downloading \(for example check the file existence based on the messahe headers only\)\.

The table will have the following columns:


1. **Time stamp** \- Current action time as number of millisecond from action start moment\.
2. **Uploaded or downloaded segments since previous entry** \- Number of segments, which was transfered after previous table entry\.
3. **Totally uploaded or downloaded segments** \- Total number of segments, which was transfered since action was began\.
4. **All segments** \- Total number of segments of file\.
5. **Uploaded or downloaded bytes since previous entry** \- Number of bytes, which was transfered after previous table entry\.
6. **Totally uploaded or downloaded bytes** \- Total number of bytes, which was transfered since action was began\.
7. **All bytes by segment count** \- All bytes to transfer based on number of segment, it will be greater than real file size, because usually the size of last segment is less than the size of each segment other than last\.
8. **All bytes by file size** \- All bytes to transfer based on the current data file size\. When uploading or comparing with file, the value meets the file size\. When downloading, the file size is less than whole file size, because one segment has not the information about file size\. In such case, the all bytes value will increase during downloading segment by segment\. It will meet the real file size after downloading the last file segment\.

The table is intended to easy detect transfer obstruction due to internet connection break or reaching account upload limit during uploading\. To visualize transfer, you can make graph, which presents the number of transfered segments or bytes during time\.

For example, using your spreadsheet application, create the chart consisting using the **Time stamp** as values of X\-axis and **Totally uploaded or downloaded segments** as Y\-axis\. It is recommended to draw the plots connected by lines\. If no transfer obstrucion was occured, the chart will have constantly increasing trend\. Othervise, each transfer obstruction will be presented as constant trent on the chart\.

## Action details and error messages

The transfer log file will not contain the action details in time, including account error messages, which are printed on the console\. To store such data, use messages log file\.




