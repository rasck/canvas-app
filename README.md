# canvas-app
An console app to fetch groups from courses on canvas.

Based on a course id it creates an excel sheet with groups.

- Build the project
- Make sure nuget packages are up to date
- Get a token from canvas: https://ucn.instructure.com/profile/settings and save the token in the App.Config file
- Run the project and follow the wizard. 
    - You need the id of the course. You can find it in the url to the course: https://ucn.instructure.com/courses/11197 <-- course id is 11197
    - Give your excel sheet a name
- In the same folder you executed the canvas app from, there should be an excel document with the groups.

