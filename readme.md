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


* **ThreadsUpload** \- Number of simultaneous threads used in uploading \(default 1\)\.
* **ThreadsDownload** \- Number of simultaneous threads used in downloading \(default 1\)\.
* **UploadGroupChange** \- Number of upload errors, after which the group of sending accounts will be changed to the nest group \(default 5\)\.
* **DownloadRetry** \- Number of retries to download the same message after download failure and reconnection\.
* **DefaultSegmentType** \- Segment type when segment type is not specified in upload command \(default 0\)\.
* **DefaultSegmentSize** \- Segment size when segment size is not specified in upload command \(default 16777216 = 16MB\)\.
* **DefaultImageSize** \- Default image size \(width\) when image size is not specified in upload command \(default 4096\)\.
* **RandomCacheStepBits** \- Number of bits to specify caching period in generating the dummy file contents \(default 25, which means 32MB\)\.
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

### General configuration checking

If you provide only first parameter, the application will print general configuration only and number of configured e\-mail accounts\.

```
BackupToMail CONFIG
```

### Account configuration checking and testing

You can provide the account numbers separated by comma \(without space separation\) as second parameter to print loaded configuration about specified account\. For print cofiguration for account 1, 2 and 4, you have to run this command:

```
BackupToMail CONFIG 1,2,4
```

You can test SMTP, IMAP and POP3 connection while configuration printing:

```
BackupToMail CONFIG 1,2,4 1
```

You can also test connection without configuration details to check if all accounts are available and see all connection failures at first glance\.

```
BackupToMail CONFIG 1,2,4 2
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
  * **0** \- The segment was should to be processet, but not processed\.
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
BackupToMail MAP 0 "D:\file.zip" "D:\file.map"
```

The only important thing about data file is file size and segment size\. In the command above, the default segment size will be used\. You can use custom segment size to calculate number of segments\.

```
BackupToMail MAP 0 "D:\file.zip" "D:\file.map" 1000000
```

You can use dummy file definition, if you know file size, in this example, the size of simulated file is 500000000 bytes:

```
BackupToMail MAP 0 "*500000000,2,," "D:\file.map" 1000000
```

The file contents are not important, so you can use any valid dummy file definition with desired size\. The only important is the number of segments of data file, so the following command will give the same result as above command:

```
BackupToMail MAP 0 "*500,2,," "D:\file.map" 1
```

If you have digest file named **file\.dig**, you can use id to read map file:

```
BackupToMail MAP 1 "D:\file.dig" "D:\file.map"
```

In this case, the segment size and number of segments will be read from the digest file, even, if you provide custom segment size\.

If the **NameSeparator** in **Config\.txt** is set, you can print information about more than one file at once command, when you set multiple data/digest files and map files\. If you assume, that the **NameSeparator** character is **&#124;**, you can print information about three files by such command:

```
BackupToMail MAP 0 "D:\file1.zip|D:\file2.zip|D:\file3.zip" "D:\file1.map|D:\file2.map|D:\file3.map"
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
8. **Segment type** \- The segment type other than default \(you can not provide segment type without providing segment size\), using one of the numbers:
  * **0** \- Binary attachment
  * **1** \- PNG image attachment
  * **2** \- Base64 in plain text body
  * **3** \- PNG image in HTML body
9. **Image width** \- The image wigth used, if segment type is **1** or **3** \(you can not provide image width without providing segment type\)\.

If item name, data file name or map file name contains spaces, you have to provide this parameter in quotation signs like "file name with spaces"\. The source and destination account list can not contain a spaces\. Below, there are some examples:

Upload **file\.zip** using **file\.map** as map file, save item named as **File** from accounts 1 and 2 to accounts 2 and 3, use default settings:

```
BackupToMail UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3
```

Upload the same file using the same accounts with provide 1000 image width and 1000000 bytes segment length:

```
BackupToMail UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3 1000000 1 1000
```

Upload the same file using four accounts in two groups to store in account 0:

```
BackupToMail UPLOAD File D:\docs\file.zip D:\docs\file.map 0,1,..,2,3 0
```

Upload **file\.zip** without map file, save item named as **File** from account 0 to account 0, use default settings:

```
BackupToMail UPLOAD File D:\docs\file.zip / 0 0
```

Upload **file\.zip** without map file, save item named as **File** from account 0 to account 0, use default settings \- alternative way:

```
BackupToMail UPLOAD File D:\docs\file.zip "" 0 0
```

Upload **file with spaces\.zip** using **file with spaces\.map** as map file, save item named as **File** as Base64 encoded in message body from account 0 to account 0\.

```
BackupToMail UPLOAD File "D:\docs by user\file with spaces.zip" "D:\docs by user\file with spaces.map" 0 0 1000000 2
```

## Upload several files at once

If the **NameSeparator** in **Config\.txt** is set, you can upload more than one file at once command, when you set multiple item names, data files and map files\. If you assume, that the **NameSeparator** character is **&#124;**, you can upload three files by such command:

```
BackupToMail UPLOAD "File1|File2|File3" "D:\file1.zip|D:\file2.zip|D:\file3.zip" "D:\file1.map|D:\file2.map|D:\file3.map" 0 0
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
BackupToMail UPLOAD "test" "*50000000,2,," "" 4 4 1000000
```

This command will generate 50 messages\.

### Step 3

Print the account contents by the following commands:

```
BackupToMail DOWNLOAD "test" "" "" 4 1 0
```

You will get the segment order in the account\. Consider, that the segment order may not be the same as uploaded\.

### Step 4

Perform clearing account using the following command, obserwing the information printed to the screen:

```
BackupToMail DOWNLOAD "test" "" "" 4 1 1,2,3,4,5,6
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
6. **Download or check mode** \- One of available modes, which uses the same principle \(some of this modes implies no whole message download\), the mode is a number from the following:
  * **0** or **10** \- Download data file \(default mode, which is used, if this parameter is not specified\)\.
  * **1** or **11** \- Check existence without body control\.
  * **2** or **12** \- Check existence with body control\.
  * **3** or **13** \- Check the header digest using data file\.
  * **4** or **14** \- Check the body contents using data file\.
  * **5** or **15** \- Download digest file\.
  * **6** or **16** \- Check the header digest using digest file\.
  * **7** or **17** \- Check the body contents using digest file\.
7. **Delete option list** \- List of values separated by commas, which indicates, which messages must be deleted \(additionaly with download/check action\):
  * **0** \- None\.
  * **1** \- Bad \- after certain number of attempts in a row\.
  * **2** \- Duplicate\.
  * **3** \- This file\.
  * **4** \- Other messages\.
  * **5** \- Other files\.
  * **6** \- Undownloadable messages \- after certain number of attempts in a row\.

Because the downloading or checking principle is browsing messages item by item \(information, which messages contains desired item, not exists\), BackupToMail can browse the message index in forward order or backward order\. The browsing order depends on downoad od check mode as following:


* Between **0** and **7** \- forward order
* Between **10** and **17** \- backward order

The browsing order is not important in most cases\. The cases, in which browsing order affects the result or working time, are for example:


* In the account, there exists messages with the same subject, due to uploading two different files, which has the same number of segments using the same item name\.
* All messages containing desired item are rathet at the index beginning or ending, but you not specify message number range\.

If item name, data file name or map file name contains spaces, you have to provide this parameter in quotation signs like "file name with spaces"\.

There are some examples, if the order is not described, it means forward order:

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages in forward order:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages in backward order:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1 10
```

Download **File** and save as **file\.zip** without map file from account 1 with reading all messages:

```
BackupToMail DOWNLOAD File D:\docs\file.zip / 1
```

Download **File** and save as **file\.zip** without map file from account 1 with reading all messages \- alternative way:

```
BackupToMail DOWNLOAD File D:\docs\file.zip "" 1
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from the first to 50:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,..50
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to the last:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..50
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50, then account 3 with reading all messages:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..50,2
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages, then account 3 with reading all messages:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,2
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50, then account 3 with reading messages from 20 to 40:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,30..50,2,20..40
```

Download **File** and save as **file with spaces\.zip** using **file with spaces\.map** as map file from account 1 with reading all messages:

```
BackupToMail DOWNLOAD File "D:\docs by user\file with spaces.zip" "D:\docs by user\file with spaces.map" 1
```

Check in forward order, if **File** item exists on account 1 with reading all messages, in this action data file name is not used:

```
BackupToMail DOWNLOAD File dummy D:\docs\file.map 1 1
```

Check in backward order, if **File** item exists on account 1 with reading all messages, in this action data file name is not used:

```
BackupToMail DOWNLOAD File dummy D:\docs\file.map 1 11
```

Check, if **File** item exists on account 1 with reading all messages, delete bad and duplicate messages of this item, in this action data file name is not used:

```
BackupToMail DOWNLOAD File dummy D:\docs\file.map 1 1 1,2
```

Delete **File** item from account 1 and 2 in forward order:

```
BackupToMail DOWNLOAD File dummy D:\docs\file.map 1,2 1 3
```

Delete **File** item from account 1 and 2 in backward order:

```
BackupToMail DOWNLOAD File dummy D:\docs\file.map 1,2 11 3
```

Download and delete **File** item from account 1 and 2:

```
BackupToMail DOWNLOAD File D:\docs\file.zip D:\docs\file.map 1,2 0 3
```

Clear account 1 and 2 \(the item name and file name are not important, any file will not be created and not tried to read in **1** or **11** mode\):

```
BackupToMail DOWNLOAD File dummy / 1,2 1 3,4,5
```

## Download several files at once

If the **NameSeparator** in **Config\.txt** is set, you can download more than one file at once command, when you set multiple item names, data files and map files\. If you assume, that the **NameSeparator** character is **&#124;**, you can download three files by such command:

```
BackupToMail DOWNLOAD "File1|File2|File3" "D:\file1.zip|D:\file2.zip|D:\file3.zip" "D:\file1.map|D:\file2.map|D:\file3.map" 0
```

The separator character can not be used in item or file name\. Otherwise, you have to change this character in **Config\.txt** and use it in command\. Every list should consist of the same items\. If not, the number of downloaded files will equal with the number of item of the shortest list\. The further items on other lists will be ignored\.

The file will be downloaded by browsing the account, so, if you want to download several files from the same account, using one command to download several files is faster, because the account will be browser once to get all files instead of several times, everytime for each file\.

## Download principle

BackupToMail will download only this segments, which are provided to download against map file\. To download whole file, be sure, that provided map file not exists or consists of **0** characters only\.

Before download, all **1** occurences in the map file will be replaced with **2**\.

If you provide more than one account, the file will be downloaded from the accounts sequentially, so some segments will be downloaded from account other than the first only when the segments does not exist on previously browsed accounts\. If all neccesary segments is downloaded, the next accounts will not be browsed\.

Within one account browsing, there will be analyzed all messages in the account\. The number of messages to browse can be limited by providing index interval described in performing command description\. You can provide the first index, the last index or both the first and last index\. It is usable, when you know approximetly the index interval, within there is file to download\.

Every browsed message information is printed and the subject is analyzed to determine, if the message seems to be contain a rewuested file\. The number of file segments is not known until there will be browsed one of the segments matching to requested item \(subject contains the digest of item name\)\. Such messages will not be downloaded immediately\. The downloading will be begin, if occurs one of the following events:


* Found as number of messages containing requested file parts do download as set number od download threads\.
* After last message to download there are browsed as number of other messages as set number od download threads\.
* After browsing the last message in the browsing iteration loop\.

The time during downloading, from creating download threads to saving to file each segment from this threads, is measused and there is encountered byten of successfully downloaded segment\. After threads end, application prints download speed\.

If internet connection lost or IMAP/POP3 server is temporally unavailable while header browsing, BackupToMail reconnects and decrements iteration index by 1 to repeat header browsing attemp of the same message\. The download is performed in separated threads and there will be done one attemp of download each message, which should be downloaded\. BackupToMail checks, that message index in every connection points to the same message\. If no, all connections will be reconnected like, in case of connection lost during message download and messade download attemp will be repeated\.

If file is downloaded without deletion options, the download process ends immediately after download last missing segment\. You can download the digest file \(mode **5**\) instead of data file \(mode **0**\), thi data segments for the digest file will be downloaded exactly by the same way as download data file\.

## Digest file

Fo any data file, you can generate the digest file, which consists of digest for each data file segment\. The first 32 characters of digest file designes the file size and segment size, each occupies 16 bytes\. The further bytes are the segment digests, each consists of 32 characters\.

To generate or check the digest file, you have provide the following parameters:


1. **DIGEST word** \- generate or check the digest file\.
2. **Mode** \- One of the following modes:
  * **0** \- Create digest file based on data file\.
  * **1** \- Check digest file against data file\.
3. **Data file name** \- The name of data file, which will used to create or check the digest file
4. **Digest file name** \- The name of the digest file\.
5. **Segment size** \- The size of one data file segment\.

The first four parameters are required and the fifth parameter is optional\. If the segment size is not provided or incorrect, there will be used the default segment size\.

To create the digest **SomeArchive\.digest** of **SomeArchive\.zip** file using 1000000 segment size, you have to execute the following command:

```
BackupToMail DIGEST 0 SomeArchive.zip SomeArchive.digest 1000000
```

There will be displayed progress of digest creation\.

To check the digest **SomeArchive\.digest** against the **SomeArchive\.zip** file using 1000000 segment size, you have to execute the following command:

```
BackupToMail DIGEST 1 SomeArchive.zip SomeArchive.digest 1000000
```

There will be displayed the following information:


* Match of the data size and segment size stored in the digest file\.
* Which digests matches the data file segments\.
* Numbers of matched and mismatched segment digest\.

The digest file can be used as data file substitute to chceck the completeness and correctness of uploaded data without the original data file, especially, when the data file is very large\. You can download the digest file \(mode **5**\) instead of data file \(mode **0**\), the digests will be generated based on the data file segments\.

## Checking file

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

# Filling missing segments

In some cases, there are possible situation, in such some of parts are missing\. Such cases can occur due to sending errors, limits or sometimes due to account errors\. In such situacion you have to reupload missing segments\. BackupToMail does not have such function, but filling can be achieved using upload and download function\.

For example, assume that, there is file named **Archive**, which exists on accounts **0** and **1**, but on both accounts some segments are missing, every segment is available from at least one account\. The repairing/filling steps are described below\.

## Detect missing segments

At the first step, you have to get information, which segments are missing, if any segment are missing\. To do this, you have to use download function to detect, which segment exists\. There are some variants of usage depending on having the file on the disk\. If map file with given name exists, remove it\.

If you do not have the file, you can use the mode 1 or 2 \(given file name does not matter, because it will not be read or written\):

```
BackupToMail DOWNLOAD Archive Archive.zip Archive.map 0 1
BackupToMail DOWNLOAD Archive Archive.zip Archive.map 0 2
```

If you have the original file \(saved as **Archive\.zip**\), you can use the mode 3 or 4:

```
BackupToMail DOWNLOAD Archive Archive.zip Archive.map 0 4
BackupToMail DOWNLOAD Archive Archive.zip Archive.map 0 4
```

If you have the digest file \(saved as **Archive\.dig**\), you can use the mode 6 or 7:

```
BackupToMail DOWNLOAD Archive Archive.dig Archive.map 0 7
BackupToMail DOWNLOAD Archive Archive.dig Archive.map 0 7
```

The generated Archive\.map file will contain information, which segments of Archive exists on account 0\.

## Download missing segments from another account

If you have original data file on the disk, you can ship this step\. If not, and you have the same file on the another account, for example in account **1**, you have to download missing segment of the file\. Downloading whole data file is not necessary to reupload missing segments\.

The map file will be modified during downloading, so you have copy the map file:

```
copy Archive.map Archive_copy.map
```

Then, tou can download missing segments using the copied map file:

```
BackupToMail DOWNLOAD Archive Archive.zip Archive_copy.map 1 0
```

If account 1 contains all missing segments, at the result you will get information, that you have all segments of the file, no missing segments\. This information is generated by analyzing map file\. The ral file will probably seem be corrupted, but this file contains only the downloaded segments, not whole data file\. If account 1 does not contain all segments, which are missing on account **0**, but you have the same file on account **2**, you can repeat download proedure on account **2**:

```
BackupToMail DOWNLOAD Archive Archive.zip Archive_copy.map 2 0
```

The segments downloaded from account 1 will not affected, the map file has information, which segments are have downloaded\.

If you download all missing segments, you can remove the **Archive\_copy\.map** file\.

## Reupload missing segments

The last step is upload missing segments on the account 0\. you have to use the map file, which was generated at the first step:

```
BackupToMail UPLOAD Archive Archive.zip Archive.map 0 0
```

There is not matter, which account you will use to upload, the destination account is important\. If you have many segments to upload, you can use multiple accounts \(for example accounts **0** and **1**\) to reduce upload obstruction\.

```
BackupToMail UPLOAD Archive Archive.zip Archive.map 0,1 0
```

You will get the information, that all segments are uploaded, because the map file will contains all segments markered as transfered\. You can remove the **Archive\.map** and **Archive\.zip** files at the moment, the files will not needed longer\.

You can check if really all segments egists using on of download/checking functions such as:

```
BackupToMail DOWNLOAD Archive Archive.zip Archive.map 0 2
```

# Message structure

While uploading file, BackupToMail creates serie of messages \(one email per file segment\), which has specified subject and contains a file segment data\.

## Subject specification

The message subject contains the **X** character exactly 7 times, which splits the subject to 8 parts\. The first \(0\) and the last \(7\) part are not used and are blank in sending messages\.

The 6 parts from 1 to 6 can contain only digits and letters from **A** to **F**\. The parts are following:


1. Digest of file name \(result of MD5 function\)\.
2. Number of this segment decreased by 1, represented by hex number, the first segment is 0\.
3. Number of all segments decreased by 1, represented by hex number\.
4. Size of this segment \(can differ from nominal segment size in last segment\) decreased by 1, represented by hex number\.
5. Nominal segment size decreased by 1, represented by hex number\.
6. Digest of segment raw content \(result of MD5 function\)\.

The mail can store file segment data inside body or as attachment\. There are supported four segment types:


* Binary attachment
* PNG image attachment
* Base64 in plain text body
* PNG image resource used in HTML body
* PNG image embedded in HTML body

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
  * 0 \- Linear congruential
  * 1 \- Fibonacci
3. **Number of bits** \- there is a number of least significant bits of state value, which are used to generate byte value:
  * 1 \- use one least significant bit, use 8 values to generate one byte\.
  * 2 \- use two least significant bits, use 4 values to generate one byte\.
  * 4 \- use least significant nibble, use 2 values to generate one byte consisting of two nibbles\.
  * 8 \- use whole value modulo by 256 as one byte\.
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
  * 2 \- Digest
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
  * 3 \- \.NET internal
3. **Generator seed **\- This parameter is optional:
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
BackupToMail FILE "*500000000,1,8,3,1,257,7,16,5" File.bin 10000000
```

## Period and statistics

Both described generators generates bytes in period, which in many cases can be short, depending on input parameters \(dummy file definition\)\. You can use file creating to get statistics\. 

The period search may take a long of time depending on file size\. The progress is displayed by **period** and **occurence** values\. The algorithm tests if file contains the period of given size, begins from 1 and ends either if period found or if tested period size reach the file size\. Within testing one period length value, program checks all occurences if the maches the first occurence\. If any occurence does not match the first occurence, the further occurences will not be tested The progress will be displayed every 1 second using timer\.

Example to create file with display simple table with file distribution and period calculation with display period distribution using detailed list without zeros:

```
BackupToMail FILE "*500000000,1,8,3,1,257,7,16,5" File.bin 0 1 2
```

# Batch operations

The following operations displays the operation parameters and waits for the confirmation by user:


* **UPLOAD** \- Uploading the file\.
* **DOWNLOAD** \- Downloading or cheching the file\.
* **DIGEST** \- Generating or cheching the digest file\.
* **FILE** \- Generating the data file based on dummy file parameters\.

If you execute one of the mentioned operations, there will be displayed the question **Do you want to continue \(Yes/No\)**\. If you write one of the following answers \(letter case is not important\), there will be interpreted as **Yes**: **1**, **T**, **TRUE**, **Y**, **YES**\. Other answer will be interpreted as **No**\.

You can ommit the confirmation if you add the BATCH word to operation word ass following:


* **UPLOADBATCH** or **BATCHUPLOAD**
* **DOWNLOADBATCH** or **BATCHDOWNLOAD**
* **DIGESTBATCH** or **BATCHDIGEST**
* **FILEBATCH** or **BATCHFILE**

This approach is very usable, when you want to run the BackupToMail from the script or batch file many times \(for example, to upload or download many files\)\.

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




