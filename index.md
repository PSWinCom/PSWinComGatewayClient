---
layout: default
---

# Installing
```
PM> Install-Package PSWinCom.Gateway.Client
```

# Sending SMS
```
  Gateway
    .Client("https://xml.pswin.com", "username", "password")
    .Send(
        new Sms("2077", "4799999999", "Test æøå")
    );
```