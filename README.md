# Advanced Auto Retry API

The Advanced Auto Retry API is used basically as a proxy between the Advanced Auto Retry plugin running on your server, and the Cloudflare GraphQL API. This runs separately from the plugin due to there not currently being a GraphQL implementation in SourcePawn. The plugin will make a request to the API for a certain IP/map combination, and the API will pull firewall events from Cloudflare to check if an event in the past x minutes matches that IP/map combo, it then returns this as a boolean to the plugin to handle.

## Requirements

- A Windows or Linux server to host the API on (or any platform you can build .NET Core for)
- A FastDL server running behind the Cloudflare CDN (the [proxied symbol](https://i.imgur.com/yPXvOvo.png) should be on for the relevant DNS entry)
- A Cloudflare Firewall Event set up to trigger for all FastDL requests (explained below)

# Installation

Download the [latest version](https://github.com/Vauff/AdvancedAutoRetry-API/releases) of the API and extract it wherever you prefer. For Linux you will use **./AdvancedAutoRetry-API** to start the API, Windows uses **AdvancedAutoRetry-API.exe**.

## Cloudflare Setup

This part will assume you already have your FastDL set up behind Cloudflare as a standard website, if you need help with that part I would just suggest googling it.

The first thing you need to do is create a firewall rule that will catch and log all requests made to your FastDL. Navigate to your Cloudflare dashboard and go to Firewall > Firewall Rules > Create a Firewall rule.

![Cloudflare Dashboard - Firewall Rules](https://i.imgur.com/N6ZSwk3.png "Cloudflare Dashboard - Firewall Rules")

Name it whatever you want, and then setup the match to check for a User Agent of "Half-Life 2". You can additionally add an OR hostname check as seen below, however this is not necessary and I only do mine this way to log requests coming from outside of game as well. Finally we want to select the Allow action, as we do not want to block these requests in any way, only create a log of them. Press Deploy when you are finished.

![Cloudflare Dashboard - Create a Firewall rule](https://i.imgur.com/6Ci0PDC.png "Cloudflare Dashboard - Create a Firewall rule")

Next we need to disable IPv6 compatibility on the site, this is necessary because CS:GO does not support IPv6 networking, and trying to match an IPv4 address with an IPv6 one will not work and the client will not be seen as having downloaded the map. Unfortunately, Cloudflare [disabled](https://blog.cloudflare.com/always-on-ipv6/) the option for this in the dashboard, but we are still able to change it using [this API call](https://api.cloudflare.com/#zone-settings-change-ipv6-setting). You can make this call using any REST client such as [Postman](https://www.postman.com/product/api-client/), or [curl](https://curl.haxx.se/download.html) in a terminal as shown in this example.

```
curl -X PATCH "https://api.cloudflare.com/client/v4/zones/023e105f4ecef8ad9ca31a8372d0c353/settings/ipv6" \
     -H "X-Auth-Email: user@example.com" \
     -H "X-Auth-Key: c2547eb745079dac9320b638f5e225cf483cc5cfdda41" \
     -H "Content-Type: application/json" \
     --data '{"value":"off"}'
```

You should pay attention to the Zone ID in the URL (023e105f4ecef8ad9ca31a8372d0c353), the X-Auth-Email and the X-Auth-Key values as you will need to change these for the API call to work. Make sure to hold on to these values too, since they are all needed in the API configuration step further down.

To find the Zone ID, navigate to your Cloudflare Dashboard homepage and click your domain. You should spot your Zone ID on the sidebar when scrolling down.

![Cloudflare Dashboard - Main page](https://i.imgur.com/GBIwKgT.png "Cloudflare Dashboard - Main page")

The X-Auth-Email value should be simple enough, in here you simply need to put the email address you use for your Cloudflare account.

X-Auth-Key can be obtained by navigating to My Profile > API Tokens > Global API Key > View

![Cloudflare Dashboard - API Tokens](https://i.imgur.com/kLu0MCF.png "Cloudflare Dashboard - API Tokens")

Finally, send the request! Afterwards, you should be able to check the Network section of your site and see the IPv6 option disabled.

![Cloudflare Dashboard - Network](https://i.imgur.com/VJh4dKB.png "Cloudflare Dashboard - Network")

## API configuration

The API ships with a appsettings.json file that is used for configuring the API. On a default install you **must** change the following values.

- **Token**: Make up your own password here that will be used to protect external access to the API, this is the value that you provide to the plugin.
- **ZoneID**: Use your Zone ID from the IPv6 step above.
- **X-AUTH-EMAIL**: Your Cloudflare accounts email address.
- **X-AUTH-KEY**: Use your X-AUTH-KEY from the IPv6 step above.


Additionally you can tweak these options to your liking.

- **GraphQLURI**: The URI that the Cloudflare GraphQL API sits at, you will probably only want to change this if Cloudflare updates it in the future.
- **FirewallEventMinutes**: How far back in minutes the API will pull firewall events for, 15 by default.
- **Debug**: Whether to log debug messages, false by default.

## Making your API accessible

By default the API will not bind to a public IP, out of the box it will bind to http://localhost:5000. if you are running the API on the same server as your game server, you can just leave it this way and provide that URL to the plugin. However, if your game server is on a different server, then you must set the **ASPNETCORE_URLS** environment variable to something like http://*:5000 to make the API bind to a public IP and be accessible from the internet. Note that if you want to use a standard port like 80, the application must have root/administrator permissions.

If you are running the API on a Linux server, I highly recommend following the steps in [this article](https://swimburger.net/blog/dotnet/how-to-run-aspnet-core-as-a-service-on-linux) (minus the Systemd integration package one) to have an ideal deploy. Additionally, you can also proxy the API behind another web server if you choose to do so.