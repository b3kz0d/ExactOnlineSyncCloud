# ExactOnline Sync Cloud

The Application is [ExactOnline API](https://start.exactonline.co.uk) client for synchronization data with Cloud Platforms such as [Dropbox API](https://www.dropbox.com/developers)

#### Getting Started

Import project into Visual Studio (or alternative IDE). Before getting started you should have developer account in both [ExactOnline API](https://start.exactonline.co.uk) and [Dropbox API](https://www.dropbox.com/developers).

#### Configure

Create TestApp and you'll retrieve clientId and clientSecret. Don't forget to configure [OAuth2](https://developers.exactonline.com/#OAuth/EOL_OAuth-Dev-Authorization.htm%3FTocPath%3DAuthorization%2520(OAuth2)%7C_____0) returnUrl and Webhook url for [Dropbox API](https://www.dropbox.com/developers). WebHook url must follow like this route pattern http://yourdomain.com/dropbox/webhook. After recieving application credentials, open `Web.config` and set `appSetting` parameters.

#### Prerequisites 

If you want to run tha application local machine you'll need to prepare the application running environment. OAuth2 and Dropbox webhook requires domain name. We suggest you to install [ngrok](http://ngrok.io/) for tunneling requests to your local machine.

#### Running the tests 

Wrote sample test senario for tesing StorageService functionality.
Tests are written using MSTest. Use any runner which supports MSTest to run tests.

#### Using the app

The application does not allow to view content without authorization. It automatically redirects to Exact Online Identity Server.
After successful authorization you should connect some of available sources to synchronize data to Exact Online. Currently, only Dropbox is implemented as a source.
In home page, you can see recently synced files.

#### Implementation details

- used [Exact Online SDK](https://github.com/exactonline/exactonline-api-dotnet-client) and [Dropbox.NET](https://github.com/dropbox/dropbox-sdk-dotnet) for communicating with EndPoints
- used InMemory storage to store tokens and user info. In production code, this may be replaced with database or cache (redis?)
- used Unity for DI
- default WebAPI template is used for project creation. It may contain unused files.
