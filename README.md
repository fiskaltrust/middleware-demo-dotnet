# middleware-de-demo

This is a demo application on how to call the fiskaltrust.Middleware. It contains examples using **GRPC**, **WCF** and **Rest** using both JSON and XML.

## Getting Started

### Prerequisites
In order to use this demo you need three things:

- the demo application
- the fiskaltrust.Middleware running on your machine
- your Cashbox Id 

The demo application is available on Github under the [Releases](https://github.com/fiskaltrust/middleware-de-demo-dotnet/releases).  
The fiskaltrust.Middleware can be downloaded from the [fiskaltrust.portal](https://portal.fiskaltrust.de/). Start it up and let it run in the background to handle your requests.   
The CashboxId is visible in the portal. It is also displayed in the startup console log of the middleware. 

## Running the Demo

The Demo app needs two startup parameters: cashbox-id and url. The Url consists of the protocol used for communication and the location. The Middleware logs out all available endpoints as configured in the portal on start. 

Startup Parameter Example:  
--cashbox-id 54c6b434-cd27-442e-b39f-0960c4ad1bda  
--url rest://localhost:1500/json/

The demo will show up a list of available demo receipts. Before execute any receipt, make sure that the TSE is initialized, by calling the initial-operation-receipt. To execute a receipt against the middleware select it by its leading number and press Enter.
This will print the example and call it on the middleware with your defined endpoint. After the middleware processed the receipt it will return the result back to the demo and prints it. To go back to the list of receipts press a random key.
