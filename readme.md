# PSWinCom Gateway Client V2

## Features
- Supports Framework 3.5 and up
  - Maximum code reuse between framework versions
- Enumerable messages and results
- MMS and SMS in the same client
  - Even mixed in same request(!)
- Shorthands for getting client and sending messages
- Supports Async when Framework is 4.5
- Supports both TCP and HTTP
  - Intelligent protocol selection (just supply uri with the proper protocol prefix)
- Multi-framework NuGet
- No external dependencies
  - Optional MMS generating helper through separate NuGet
- NuGet includes Gateway Client V1 for .NET 2.0 support

## Installing

```
	PM> Install-Package PSWinCom.Gateway.Client
```


## Samples

### Sending SMS

```
Gateway
    .Client("https://xml.pswin.com", "username", "password")
	.Send(
	    new Sms("2077", "4799999999", "Test æøå")
	);
```

### Sending MMS

First install the MMS package

```
PM> Install-Package PSWinCom.Gateway.Client.MMS
```

Then use this to create MMS file

```
Gateway
    .Client("tcp://sms3.pswin.com:1111", "username", "password")
	.Send(
	    new Mms("2077", "4799999999", "Test æøå",
			new MmsFile(
	            MmsPart.FromFile(@"testfiles\pswinstache.jpg"),
	            MmsPart.FromText("Husk å støtte PSWin'stache", "message.txt")
	        )
	    )
    );
```

NOTE! If you already have a prepacked zip file for sending mms you do not need to install the MMS package, you can just read the file contents into a byte array and assign this byte array to the MmsData property.

### Sending multiple/mixed messages

```
Gateway
    .Client("username", "password")
	.Send(
	    new Mms {
	        Text = "Test æøå",
	        ReceiverNumber = "4799999999",
	        SenderNumber = "26112",
	        MmsData = new MmsFile(
	            MmsPart.FromFile(@"testfiles\pswinstache.jpg"),
	            MmsPart.FromText("Husk å støtte PSWin'stache", "message.txt")
	        )
	    },
	    new Sms("26112", "4799999998", "Test æøå")
    );
```

### Handling response

```
Gateway
    .Client("http://xml.pswin.com", "username", "password")
    .Send(
        new Sms
        {
            Text = "Test æøå",
            ReceiverNumber = "4799999999",
            SenderNumber = "26112",
            UserReference = "my-ref-1"
        },
        new Sms
        {
            Text = "Test æøå",
            ReceiverNumber = "4799999998",
            SenderNumber = "26112",
            UserReference = "my-ref-2"
        }
    )
    .Results
    .Each((r) =>
    {
		Console.WriteLine("My ref: {0} Gateway ref: {1} Status: {2} Message: {3} Receiver: {4}",
			r.UserReference,
			r.GatewayReference,
			r.Status,
			r.StatusText,
			r.Message.ReceiverNumber);
    });
```

### Expressive client creation

```
Gateway
	.Client("https://xml.pswin.com")
	.WithLogin("username", "password"
	.Send(
		...
	);
```

### Sending messages async (.net 4.5 and up)

```
var response = await Gateway
	.Client("https://xml.pswin.com", "username", "password")
	.SendAsync(
		new Sms { ... }
	);

response
	.Results
	.Each((r) =>
    {
		Console.WriteLine("My ref: {0} Gateway ref: {1} Status: {2} Message: {3} Receiver: {4}",
			r.UserReference,
			r.GatewayReference,
			r.Status,
			r.StatusText,
			r.Message.ReceiverNumber);
    });
```
