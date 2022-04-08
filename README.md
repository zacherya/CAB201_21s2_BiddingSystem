# QUT CAB201 2021 Semester 2 Object Oriented Project "BiddingSystem"
> C# Console app bidding system, developed in Visual Studio 2019 using .Net Core 5


## Demo Envirorment
A demo envirorment has been provided which is generated with random names, products and bids upon execution of the program.
To initiate the demo envirorment the following arguments must be passed:
    `/d` - Demo 
        OR
    `/demo` - Demo
To login with any demo accounts you must use any of the names below:
    `"Bob", "Jane", "Mark", "Sally", "Zoe", "Zac", "Fiona", "David"`
followed by `"@email.com"` as the email and all have the same password `"Pass123$"`.
You do not need to know the last name as it's randomly generated at runtime.

## Screenshots
![image](https://user-images.githubusercontent.com/77133479/162386428-93e3cca6-7063-4f72-aaf2-a9be35ab4e62.png)
![image](https://user-images.githubusercontent.com/77133479/162386583-93fdafde-fa8a-465d-a8bc-1b9cc76f28b0.png)
![image](https://user-images.githubusercontent.com/77133479/162386725-680849e2-ad15-4e12-aaa2-caff9f5cc02f.png)
![image](https://user-images.githubusercontent.com/77133479/162386842-bb3de4a3-a0d4-49d6-9d40-c1e6ba433aa8.png)

## Data Providers
Used to separate functionality, UI from the data management side of the application. 
Expandable for different future interface e.g Web, WPF

## Interface Management
Used to manipulate the current chosen console interface to achieve functionality through menus.
A modular interface object system has been used to create room for more potential menus and separation of functionality across menus

## Table Generator
Used as an extension to the console interface management.
It will display ambigous types of data lists by converting them into table strings.

## Identity Controller
Used to manage the user authentication and segregate security measures to authenticate users

## Menu Constructor
Used as an extension to the Interface object to build menus for the current instance and communicate to and from parent and child classes
