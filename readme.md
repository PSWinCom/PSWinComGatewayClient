# PSWinCom Gateway Client V2
This is Henrik's entry to the Gateway Client competition. 

## Features
- Supports Framework 3.5 and up (we could event throw in the original client for 2.0 support)
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


## Samples

### Sending SMS

```
Gateway
    .Client("http://sms3.pswin.com/sms", "username", "password")
	.Send(
	    new SmsMessage { 
	        Text = "Test æøå", 
	        ReceiverNumber = "4799999999", 
	        SenderNumber = "2077"
	    }
	);
```

### Sending MMS

```
Gateway
    .Client("tcp://sms3.pswin.com:1111", "username", "password")
	.Send(
	    new MmsMessage { 
	        Text = "Test æøå", 
	        ReceiverNumber = "4799999999", 
	        SenderNumber = "2077", 
	        MmsData = new MmsFile(
	            MmsPart.FromFile(@"testfiles\pswinstache.jpg"),
	            MmsPart.FromText("Husk å støtte PSWin'stache", "message.txt")
	        )
	    }
    );
```

### Sending multiple/mixed messages

```
Gateway
    .Client("username", "password")
	.Send(
	    new MmsMessage { 
	        Text = "Test æøå", 
	        ReceiverNumber = "4799999999", 
	        SenderNumber = "26112", 
	        MmsData = new MmsFile(
	            MmsPart.FromFile(@"testfiles\pswinstache.jpg"),
	            MmsPart.FromText("Husk å støtte PSWin'stache", "message.txt")
	        )
	    },
	    new SmsMessage { 
	        Text = "Test æøå", 
	        ReceiverNumber = "4799999998", 
	        SenderNumber = "26112"
	    }
    );
```

### Handling response

```
Gateway
    .Client("http://sms3.pswin.com/sms", "grotle", "prosyna3")
    .Send(
        new SmsMessage
        {
            Text = "Test æøå",
            ReceiverNumber = "4799999999",
            SenderNumber = "26112",
            UserReference = "my-ref-1"
        },
        new SmsMessage
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
        Console.WriteLine("My ref: {0} Gateway ref: {1} Status: {2} Message: {3}",
            r.UserReference,
            r.GatewayReference,
            r.Status,
            r.Message);
    });
```

### Expressive client creation

```
Gateway
	.Client("http://sms.pswin.com/sms")
	.WithLogin("username", "password"
	.Send(
		...
	);
```

