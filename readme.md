# BackupToMail overwiev

This application is a command\-line application, which allows to upload any large file to any mailbox by splitting to segments\. Unlike Peer2Mail file sharing technology, there are some differences comparing to ordinary P2M applications:


* BackupToMail is intended to keep own data \(like backup\) in the own mailboxex\.
* BackupToMail does not generate hash information, so it is not intended to share your files to untrusted people and there is not data lose risk by losing hash information\.
* BackupToMail does not support in creating e\-mail accounts \(some P2M application can automatically create free of charge accounts on some servers\)\.
* BackupToMail can support any mailbox by POP3 or IMAP protocol to download file, and SMTP protocol to upload file\.
* BackupToMail works in console, so you can use it through text terminal connection \(telnet or ssh\) and you can execute download or upload from other application\.
* The message structure and upload/download technology is simple and easy to reproduce\.

If you run application without parameters or with unsupported action parameter, the application will print short description of command line syntax\. The action parameter is the first parameter and there is not case\-sensitive\.

# Configuration

BackupToMail uses the **Config\.txt** file to get configuration\. This file is a textual file, which contains the **parameter=value** lines, which defines general settings and account configuration\. The lines, which are not contains the **=** sign are ignored\.

## General settings

The general settings in **Config\.txt** file are following:


* **ThreadsUpload** \- Number of simultaneous threads used in uploading \(default 1\)\.
* **ThreadsDownload** \- Number of simultaneous threads used in downloading \(default 1\)\.
* **DefaultSegmentType** \- Segment type when segment type is not specified in upload command \(default 0\)\.
* **DefaultSegmentSize** \- Segment size when segment size is not specified in upload command \(default 16777216 = 16MB\)\.
* **DefaultImageSize** \- Default image size \(width\) when image size is not specified in upload command \(default 4096\)\.

When certain parameter is not set or has incorrect value, the default value will be used\.

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
* **Mail0Pop3Use \(0/1\)** \- Use POP3 instead of IMAP when download\.
* **Mail0SmtpConnect \(0/1\)** \- Connect to SMTP server of this account directly before sending mail and disconnect after mail sending\.

The parameters indicated as **0/1** can have only **0** \(false\) or **1** \(true\) value\. The default value \(if parameter is not provided\) is **0**\. The account 1 has the same parameters, but with **Mail1** prefix istead of **Mail0** prefix\. Configuration is loaded as iterated loop while **Mail\_Address** is not blank\. If ypu define account 0, account 1 and account 3, ommiting account 2, the BackupToMail will load configuration for account 0 and account 1 only\.

## Configuration checking

You can check configuration and test accounts using **CONFIG** parameter\.

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

You can test SMTP and IMAP/POP3 connection, if you input CONFIGTEST or TESTCONFIG parameter instead of CONFIG:

```
BackupToMail CONFIGTEST 1,2,4
```

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

# Uploading file

To upload file, you have to access to at least one account with SMTP server and define destination accounts\. The destination account can be the same as the source account\. Before uploading file, it is highly recommended to:


* Turn off spam filter on all your destination accounts or configure spam filter bypass for messages sent from adresses of every your source account\.
* Turn off body modifiers such as signature footer when you want to upload as Base64 or PNG image in body, without attachment\.
* Turn off autoresponder on every destination account if such service is on\.
* Encrypt file or archive, the BackupToMail does not support encryption, there are several archive formats, which supports encryption, such as ZIP, 7Z and RAR\.

## Performing upload

To upload file, you have to run BackupToMail with the following parameters, the first four parameters are required, the last three parameter is optional, but every optional parameter must be provided, if you want to provide a further parameter:


1. **UPLOAD word** \- Perform upload action\.
2. **Item name** \- Item name \(identifier\) used on account \(it not must be the same as file name\)\.
3. **Data file path and name** \- Path and name of file, which you want to upload\.
4. **Map file path and name** \- Path and name of file, which you want to upload\.
5. **Source account list** \- List of source accounts, which will be used to send messages\.
6. **Destination account list** \- List of destination accounts, which will be used provide message reipts into **To** field\.
7. **Segment size** \- The size of on segment other than default\.
8. **Segment type** \- The segment type other than default \(you can not provide segment type without providing segment size\), using one of the numbers:
  * **0** \- Binary attachment
  * **1** \- PNG image attachment
  * **2** \- Base64 in plain text body
  * **3** \- PNG image in HTML body
9. **Image width** \- The image wigth used, if segment type is **1** or **3** \(you can not provide image width without providing segment type\)\.

If item name, data file name or map file name contains spaces, you have to provide this parameter in quotation signs like "file name with spaces"\. The source and destination account list can not contain a spaces\. Below, there are some examples:

Upload **file\.zip** with use **file\.map** as **File** from accounts 1 and 2 to accounts 2 and 3, use default settings:

```
BackupToMail UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3
```

Upload the same file using the same account with provide 1000 image width and 1000000 bytes segment length:

```
BackupToMail UPLOAD File D:\docs\file.zip D:\docs\file.map 1,2 2,3 1000000 1 1000
```

Upload **file with spaces\.zip** with use **file with spaces\.map** as **File** as Base64 encoded in message body from account 0 to account 0\.

```
BackupToMail UPLOAD File "D:\docs by user\file with spaces.zip" "D:\docs by user\file with spaces.map" 0 0 1000000 2
```

After executing command, aplication will print action parameters as will be used, so you can check if command line parameters are correct\. You have to confirm action by entering one of the following \(case insensitive\): **1**, **T**, **Y**, **TRUE**, **YES**\. To ommit confirmation, use **BACHUPLOAD** or **UPLOADBATCH** instead of **UPLOAD** parameter\.

## Upload principle

BackupToMail will upload only this segments, which are provided to upload against map file\. To upload whole file, be sure, that provided map file not exists or consists of **0** characters only\.

Before upload, all **1** occurences in the map file will be replaced with **2**\.

The number of segments of whole file ic calculated based on data file size and one segment size\. The upload loop is iterated on all segments with printing informaton, which segment will be uploaded\. If there are found the same number of segments to uploads as number of upload threads, the application will assign source account to every segment to upload and the segments will be uploaded simultaneously in separate threads \(the one attemp will take place\)\. The time of preparing and sending mails will be measured and after upload attemps all segments, there will be printed upload speed based on successfully uploaded segments\.

While every segment of the threads upload failed, the application will change source account assignment and repeat upload attemp\. If some of all segments was uploaded, there will be read only as number of the segments as number of threads subtracted with number of upload failed segments\. Every failed upload will cause upload attemp repeat \(with changing source account assignment\) until such segment will be successfully uploaded\. If none of account is available due to reach sending limits or internet connection break, this uploading attemps will repeaded infinitely, until sending limits on account is reset and internet connection is recovered\. 

After iteration of last segment, there will be performed uploading all not uploaded segment by the same way as uploading segment during iteration through segments\. It is no possible, that this action is end before uploading all segments\. Eventually, you can break this action by killing the BackupToMail process, the uploaded segments already will be denoted as **1** character in map file\.\.

# Downloading or checking file

To download or check file, you have to access to at least one account with IMAP or POP3 server, which keeps uploaded file\. Both actions uses the same mechanism with slightly work differences\. During downloading or checking it is possible deletion of specified kind of messages\.

## Performing download/check

To download or check file, you have to run BackupToMail with the following parameters, the first five parameters are required, the last two parameter is optional, but every optional parameter must be provided, if you want to provide a further parameter:


1. **DOWNLOAD word** \- perform download or check action\.
2. **Item name** \- Item name \(identifier\) used on account \(it not must be the same as file name\)\.
3. **Data file path and name** \- Path and name of file, which you want to upload\.
4. **Map file path and name** \- Path and name of file, which you want to upload\.
5. **Source account list with item index intervals** \- Account list separated by commas, but pair of numbers separated by two dots \(**\.\.**\) or one number and two dots is interpreted as index interval filter \(see examples\)\.
6. **Download or check mode** \- One of available modes, which uses the same principle \(some of this modes implies no whole message download\), the mode is a number from the following:
  * **0** \- Download file \(default mode, whis is used, if this parameter is not specified\)\.
  * **1** \- Check existence without body control\.
  * **2** \- Check existence with body control\.
  * **3** \- Compare with header digest\.
  * **4** \- Compare with body contentx\.
7. **Delete option list** \- List of values separated by commas, which indicates, which messages must be deleted \(additionaly with download/check action\):
  * **0** \- None\.
  * **1** \- Bad\.
  * **2** \- Duplicate\.
  * **3** \- This file\.
  * **4** \- Other messages\.
  * **5** \- Other files\.

If item name, data file name or map file name contains spaces, you have to provide this parameter in quotation signs like "file name with spaces"\. There are some examples:

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from the first to 50:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,..50
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to the last:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,30..
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,30..50
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50, then account 3 with reading all messages:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,30..50,2
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading all messages, then account 3 with reading all messages:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,2
```

Download **File** and save as **file\.zip** using **file\.map** as map file from account 1 with reading messages from 30 to 50, then account 3 with reading messages from 20 to 40:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,30..50,2,20..40
```

Download **File** and save as **file with spaces\.zip** using **file with spaces\.map** as map file from account 1 with reading all messages:

```
BackupToMail DOWNLOAD file "D:\docs by user\file with spaces.zip" "D:\docs by user\file with spaces.map" 1
```

Check, if **File** item exists on account 1 with reading all messages, in this action data file name is not used:

```
BackupToMail DOWNLOAD file dummy D:\docs\file.map 1 1
```

Check, if **File** item exists on account 1 with reading all messages, delete bad and duplicate messages of this item, in this action data file name is not used:

```
BackupToMail DOWNLOAD file dummy D:\docs\file.map 1 1 1,2
```

Delete **File** item from account 1 and 2:

```
BackupToMail DOWNLOAD file dummy D:\docs\file.map 1,2 1 3
```

Download and delete **File** item from account 1 and 2:

```
BackupToMail DOWNLOAD file D:\docs\file.zip D:\docs\file.map 1,2 0 3
```

Clear account 1 and 2:

```
BackupToMail CHECK file dummy D:\docs\file.map 1,2 1 3,4,5
```

After executing command, aplication will print action parameters as will be used, so you can check if command line parameters are correct\. You have to confirm action by entering one of the following \(case insensitive\): **1**, **T**, **Y**, **TRUE**, **YES**\. To ommit confirmation, use **BACHDOWNLOAD** or **DOWNLOADBATCH** instead of **DOWNLOAD** parameter\.

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

If file is downloaded without deletion options, the download process ends immediately after download last missing segment\.

## Checking file

BackupToMail offers checking checking or correctness of uploaded file by four ways:


* Check existence without body control\.
* Check existence with body control\.
* Compare with header digest\.
* Compare with body contents\.

The checking principle is the same as download principle and uses the same functionality\. There are difference comparing to download: 


* Reading data file or no using data file instead of writing data file\.
* Displaying check results\.
* Process takes place to planned end, without breaking after checking all unchecked segments\.
* Some check modes takes place bases on message headers instead of downloading whole message\.

After checking, the transfer speed is displayes only for downloaded data bytes, while you perform the check mode, which requires download data\. The header data is not encountered to downloaded bytes\.Check existence without body controlThere is the simpliest check type, it nos uses the data file name \(this parameter is ignored, although it must be provided\)\. BackupToMail researches headers of all messages, which matches to requested item name, like download action\. After find some file segment, application knowns the number of file segments\. This option can detect the following things: 


* Existence of some segments of requested file\.
* Existence of all or not all segments of file\.
* Duplicate segments\.
* Equality of number of segments in header of each existing segment \(the first found segment determines the correct number\)\.
* Equality of nominal segment size in header of each existing segment \(the first found segment determines the correct size\)\.

Thich checks does not download segments and is also useful to deletion messages\.Check existence with body controlThe check type is similar to **Check existence without body control**, but additionally downloads segment\. This can detect the same things as above and additionally the following things: 


* The message contains readable segment bytes and is useful to download segment\.
* Compares real segment digest to digest provided in header\.

The check mode is very similar to download, the only difference is no saving the downloaded data\.Compare with header digestThis mode reads data from provided local data file name and calculates the digest based on the file for every correct message, withoud browsing body or attachment\. The calculated digest is compared with digest from header and if the digest differs, this messages is treated as bad\.This mode does not download segments from messages, it can detect: 


* Corrupted and incorrect messages\.
* Difference between local file contents and message contents by assuming that message content digest equals to digest saved in message header\.

Compare with body contents like **Compare with header digest**, this mode also reads the local file, but downloads segment from body \(attachment or text\) and compares the downloaded segment with the same segment from local file\. If differs, this message is bad\. 

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


