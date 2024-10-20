import sql from "mssql";
import {execaCommand} from 'execa';

/**
 * This script prints out a JSON object in the form described by
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/85759 :

    [
      {
        "environment": "prod",
        "customerId": "...",
        "customerName": "Investa",
        "siteIds": [
          "site id 1",
          "site id 2",
          "site id 3",
          "site id 3",
        ]
      },
      {
        "environment": "prod",
        "customerId": "...",
        "customerName": "Brookfield",
        "siteIds": [
          // ...
        ]
      },
      // ...
    ]

 * You will need access to all three production SQL databases. This will require a PIM
 * and three messages to Wilbur.
 *
 * It takes no arguments, so usage is just `npm install; node getSiteIds.js`.
 */

function connect({ server, database, authToken }) {
  // Note: do *not* use sql.connect!
  // https://github.com/tediousjs/node-mssql#the-global-connection-pool
  return new sql.ConnectionPool({
    server,
    port: 1433,
    database,
    authentication: {
      type: 'azure-active-directory-access-token',
      options: {
        token: authToken,
      }
    },
    options: {
      encrypt: true,
      requestTimeout: 99999
    }
  }).connect();
}

const databaseHostNames = [
  "wil-prd-plt-aue2-sql.database.windows.net",
  "wil-prd-plt-eu22-sql.database.windows.net",
  "wil-prd-plt-weu2-sql.database.windows.net",
];

async function main() {
  const authToken = (await execaCommand(
    'az account get-access-token --resource https://database.windows.net/ --query "accessToken" --output tsv'
  )).stdout;

  const out = [];

  for (const dbHostname of databaseHostNames) {
    const directoryCoreDbConnection = await connect({
      database: "DirectoryCoreDb",
      server: dbHostname,
      authToken
    });
    const siteCoreDbConnection = await connect({
      database: "SiteCoreDb",
      server: dbHostname,
      authToken
    });

    const customers = (await new sql.Request(directoryCoreDbConnection).query`SELECT * FROM Customers`).recordset;
    const sites = (await new sql.Request(siteCoreDbConnection).query`SELECT * FROM Sites`).recordset;

    for (const customer of customers) {
      out.push({
        environment: "prod",
        customerId: customer.Id,
        customerName: customer.Name,
        siteIds: sites.filter(s => s.CustomerId === customer.Id).map(s => s.Id),
      });
    }
  }

  console.log(JSON.stringify(out, null, 4));
}

main();
