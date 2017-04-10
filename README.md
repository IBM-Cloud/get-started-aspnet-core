# Getting started with ASP.NET on Bluemix
This guide will take you through the steps to get started on Bluemix with the help of a sample application. In 10 minutes or less, you’ll learn to:
- Set up a development environment
- Download sample code
- Run the application locally
- Run the application on Bluemix Cloud Foundry
- Add a Bluemix Database service
- Connect to the database from your local application


## Prerequisites

You'll need the following:
* [Bluemix account](https://console.ng.bluemix.net/registration/)
* [Cloud Foundry CLI](https://github.com/cloudfoundry/cli#downloads)
* [Git](https://git-scm.com/downloads)
* Install .NET Core 1.0.0-preview4-004233 SDK from the [preview4 download page](https://github.com/dotnet/core/blob/master/release-notes/download-archives/preview4-download.md) instructions.
* Install the latest .NET Core Runtime from the [dot.net website](https://www.microsoft.com/net/download/core#/runtime)

## 1. Clone the sample app

Now you're ready to start working with the app. Clone the repository and change to the directory where the sample app is located.
  ```
git clone https://github.com/IBM-Bluemix/get-started-aspnet-core
cd get-started-aspnet-core
  ```

## 2. Run the app locally
{: #run_locally}

Run the app.
  ```
cd src/GetStartedDotnet
dotnet restore
dotnet run
  ```

View your app at: http://localhost:5000/

## 3. Prepare the app for deployment

To deploy to Bluemix, it can be helpful to set up a manifest.yml file. One is provided for you with the sample. Take a moment to look at it.

The manifest.yml includes basic information about your app, such as the name, how much memory to allocate for each instance and the route. In this manifest.yml **random-route: true** generates a random route for your app to prevent your route from colliding with others.  You can replace **random-route: true** with **host: myChosenHostName**, supplying a host name of your choice. [Learn more...](https://console.bluemix.net/docs/manageapps/depapps.html#appmanifest)
 ```
 applications:
 - name: GetStartedDotnet
   random-route: true
   memory: 256M
 ```

## 4. Deploy the app

You can use the Cloud Foundry CLI to deploy apps.

To start, login to your Bluemix account:
  ```
cf login
  ```

Choose your API endpoint
  ```
cf api <API-endpoint>
  ```

Replace the *API-endpoint* in the command with an API endpoint from the following list.

|URL                             |Region          |
|:-------------------------------|:---------------|
| https://api.ng.bluemix.net     | US South       |
| https://api.eu-gb.bluemix.net  | United Kingdom |
| https://api.au-syd.bluemix.net | Sydney         |

Be sure you are in the main directory, **aspnet-core-helloworld**, for your application.

Push your application to Bluemix
  ```
cf push
  ```

This can take a minute. If there is an error in the deployment process you can use the command `cf logs <Your-App-Name> --recent` to troubleshoot.

When deployment completes you should see a message indicating that your app is running.  View your app at the URL listed in the output of the push command.  You can also issue the
  ```
cf apps
  ```
  command to view your apps status and see the URL.

## 5. Connect a MySQL database

Next, we'll add a ClearDB MySQL database to this application and set up the application so that it can run locally and on Bluemix.

1. Log in to Bluemix in your Browser. Browse to the `Dashboard`. Select your application by clicking on its name in the `Name` column.
2. Click on `Connections` then `Connect new`.
2. In the `Data & Analytics` section, select `ClearDB MySQL Database` and `Create` the service.
3. Select `Restage` when prompted. Bluemix will restart your application and provide the database credentials to your application using the `VCAP_SERVICES` environment variable. This environment variable is only available to the application when it is running on Bluemix.

Environment variables enable you to separate deployment settings from your source code. For example, instead of hardcoding a database password, you can store this in an environment variable which you reference in your source code. [Learn more...](/docs/manageapps/depapps.html#app_env)

## 6. Use the database locally

We're now going to update your local code to point to this database. We'll store the credentials for the services in a json file. This file will get used ONLY when the application is running locally. When running in Bluemix, the credentials will be read from the VCAP_SERVICES environment variable.

1. Create the file src/GetStartedDotnet/vcap-local.json

2. In your browser open the Bluemix UI, select your App -> Connections -> ClearDB MySQL Database -> View Credentials

3. Copy and paste the entire json object from the credentials to the `vcap-local.json` file and save the changes.  The result will be something like:
  ```
  {
  "cleardb": [
    {
      "credentials": {
        ...
        "uri": "mysql://user:password@some-hostname.cleardb.net:3306/database-name?reconnect=true",
        ...
      },
      ...
      "name": "My ClearDB service instance name",
      ...
    }
  ]
}
  ```

4. Restart your application (if it is still running).

  Refresh your browser view at: http://localhost:5000/. Any names you enter into the app will now get added to the database.

  Your local app and the Bluemix app are sharing the database.  View your Bluemix app at the URL listed in the output of the push command from above.  Names you add from either app should appear in both when you refresh the browsers.

Remember if you don't need your app live, stop it so you don't incur any unexpected charges.
