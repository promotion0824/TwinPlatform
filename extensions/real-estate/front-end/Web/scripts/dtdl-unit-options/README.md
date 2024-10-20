The original source for these definitions comes from

https://ridleyco.sharepoint.com/sites/MicrosoftWillow/Shared%20Documents/Forms/AllItems.aspx?id=%2Fsites%2FMicrosoftWillow%2FShared%20Documents%2FProduct%20and%20Engineering%2FADT%20Technical%20Docs%2FDTDL%20v3%2FDTDL%2EquantitativeTypes%2Ev1%2Emd&parent=%2Fsites%2FMicrosoftWillow%2FShared%20Documents%2FProduct%20and%20Engineering%2FADT%20Technical%20Docs%2FDTDL%20v3

Rick Szcodronski took the above and added the display names, putting them in this spreadsheet:

https://ridleyco.sharepoint.com/:x:/r/sites/MicrosoftWillow/_layouts/15/Doc.aspx?sourcedoc=%7BCB4F7A51-3C3D-4375-A131-08385F1B6DD4%7D&file=DTDLv3%20Quantitative%20Types%20and%20Units.xlsx&wdOrigin=TEAMS-WEB.teams.fileLink&action=default&mobileredirect=true

We take the data from this spreadsheet and generate enum options from it for the UI.

To update the UI's values from the spreadsheet, do this:

1. Open the spreadsheet
2. Hit Ctrl+A to select all the cells
3. Hit Ctrl+C to copy all the cells (do not use Save As since that will download an .xlsx file)
4. Open the `units.tsv` file and replace the contents of the file with what you copied
5. In the `extensions/real-estate/front-end/Web/scripts/unit-options` directory:

- Run `npm install`
- Run `node unitValues.js`

This will overwrite the contents of
`extensions/real-estate/front-end/Web/packages/common/src/twins/view/unitVals.json`, which
is imported by the app. You can now test the results and commit the file if you
are happy.
