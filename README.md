This application is created on WPF using Revit API and represents the simplest example of developing a working add-in application 
  for [Autodesk Revit](https://www.autodesk.com/products/revit/architecture) application (engineer design and construction documentation tool).

Application interacts with the Sheets (Autodesk Revit elements - it is a representation of paper sheets containing drawings and tables required for construction)

![image](https://github.com/BimDevCode/SheetWork_RevitAddin/assets/86651846/62b916ec-6212-4284-9d89-b08c24717e2f)
HOME PAGE

**Business features**:
- Viewing the names of existing sheets with their parameters in DataGrid.
- Ability to export these sheets to Excel with parameters
referring to these sheets.
- Ability to Copy sheets.
- Ability to Delete sheets.
- Ability to Rename sheets.
- Ability to rename sheets with a specified suffix and prefix.
- Ability to set sheet number auto-numbering (incrementation) when copying sheets.

**Design features**
- DI using Host service
- Page Navigation with Page Service
- Using Community MVVM Toolkit
- Validation on Value Existing
- Context Menu for actions
- Using snack bar for notifications
- Using async Excel export
- Automated library file configuration on startup
