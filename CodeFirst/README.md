# Day-4
# Code First 
<br>
means to start from code not from designing database
helps on:
  - if i want to work on different data (Database, XML files or data from mem)

<hr>

### **First** we need to start creating the database from .Net itself
   1. create 2 normal **Department**, **Employee** classes with their props  
   2. create **Context** class, and inherit from <u>**DbContext**</u>
      - by default in EF the package "DbContext" is not exist so we will download it through command line of package manager **NuGet package manager**
      - ![](Pasted%20image%2020260923181527.png) (Ef6)
      - ```cs
        // must use this lib
        using System.Data.Entity; 
        internal class Context : DbContext {
        
        }
        ```

at the class **Context**
- create 2 DbSet for Department and Employee

```cs

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
```

<hr>

## **Migration**
- to link the app to db
- when update on classes that will also update the db 
<br>
> through ( tools --> NuGet package manager --> Package Manager Console )
- 3 Important commands :
  1. this command written only one time on Project level
     `enable-migrations`
  2. `add-migration init` 
  3. `update-databse` --> to reflect the changes to database

<br>

> every time we wanna reflect changes to database we must write the last two commands 

<hr>

## Steps:
1. create the classes we need with its props 
2. create class **Context** 
   . this is the layer between client and server
   . adding the connection string through the ctor of Context class  
   . inherit from DbContext + downloading the pkg of *EF6*
3. specify the DbSets we need for our example : Employees from Employee and Departments from Department 
4. the three commands 

<hr>

## Rules (what happened behind the scene)
 After the command *add-migration init* the context creates two tables
- At `EntityFW\CodeFirst\CodeFirst\Migrations\202609231540529_init.cs` :
  **Departments** and **Employees**, <u>Why?</u>
  - based on DbSets at the context it decides what exactly the classes will be added as a tables 
- By default when i create table, its name will be the plural of the class name 
  - so the name is based on the class name not the DbSet name 
- By default according to the type of the property 
  - value type will not allow null 
  - ref type will allow null 
- By default if there is **ID/id int/long** the EF will consider it as a **Primary key**
- the file `202609231540529_init.cs` can be deleted without affecting the project

<hr>

## updating the classes and how to reflect the changes to databse

#### adding a new prop in Employee class


```cs
public string Address { get; set; }
```

<br>

- the command `add-migration updaes` will create a new cs file  contains
  ```cs
          public override void Up()
        {
            AddColumn("dbo.Employees", "Address", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Address");
        }
  ```

> [!NOTE]
> `public override void Down()` when i wanna revoke the changes 

<br>

- every time EF compare the structure of the classes and the last snapshot in the table `__MigrationHistory`
  - that table contains snapshots carries the last database structure  
- `update-database` will update the database and add the new column 
  and add a new snapshot at migration history table 

<br>

> [!NOTE]
> snapshot at migrationHistory table does not track the updates happen on the side of server (database)

<hr>

#### query to insert data 

```cs
      Context context = new Context();

      context.Departments.Add(new Department {
        Name = "SD"
      });

      context.SaveChanges();
```

