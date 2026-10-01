# EF Core — Video 1 Study Notes

A concept-first reference for the first EF Core video, following the order of my original notes and using the entities, configuration, queries, and migrations from my project.

**Repository:** [EpicFailure-afk/EF-Core](https://github.com/EpicFailure-afk/EF-Core)

**Scope:** configuration, migrations, Fluent API, conventions, Data Annotations, one-to-many relationships, and the introduction to eager loading with `Include` and `ThenInclude`.

> **How to read these notes**
> The main explanations come from my notes, the transcript, and the supplied local code. **Correction**, **Version note**, and **Extra clarification** identify additions that resolve an inaccurate statement or explain behavior beyond the demonstrated example. Microsoft documentation is linked beside those additions. The GitHub page could not be retrieved during preparation; code and migration details were checked against the supplied local repository.

## Contents

1. [Versions: EF6 and EF Core](#1-versions-ef6-and-ef-core)
2. [How to configure EF Core](#2-how-to-configure-ef-core)
3. [Defining the connection string](#3-defining-the-connection-string)
4. [Migrations and the model snapshot](#4-migrations-and-the-model-snapshot)
5. [Important commands till now](#5-important-commands-till-now)
6. [Fluent API](#6-fluent-api)
7. [Changing defaults and configuration by convention](#7-changing-defaults-and-configuration-by-convention)
8. [Composite key](#8-composite-key)
9. [Revision on Data Annotations](#9-revision-on-data-annotations)
10. [Rest of the relationships](#10-rest-of-the-relationships)
11. [What happens when the model and database differ?](#11-what-happens-when-the-model-and-database-differ)
12. [Lazy loading and loading related data](#12-lazy-loading-and-loading-related-data)
13. [EF6 vs. EF Core comparison](#13-ef6-vs-ef-core-comparison)
14. [Revision questions](#14-revision-questions)

---

## 1. Versions: EF6 and EF Core

First, separate the runtime from the data-access library:

| Name | What it represents |
| --- | --- |
| .NET Framework / .NET | The platform used to build and run the application |
| EF6 / EF Core | The library used to map entities and work with a database |

The video introduces the move from .NET Core 3.1 to the .NET 5 naming, then uses .NET 5 for its examples. My supplied project targets `net10.0` and references EF Core packages at version `10.0.12`.

**EF Core is still called EF Core.** It does not become “EF6” when an application uses .NET 6.

### Database providers

A **provider** lets EF work with a particular database. This project uses SQL Server, so it uses the SQL Server provider. The lecture also mentions Oracle, SQLite, and the non-relational Azure Cosmos DB provider as examples of other choices.

> **Correction:** EF6 is not limited to SQL Server. Other EF6 providers exist, including third-party providers. “We used SQL Server with EF6” describes the earlier course example, not the full capabilities of EF6. [Microsoft: EF6 providers](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/providers/)

The useful lesson is to identify both the EF library and the provider the application needs.

---

## 2. How to configure EF Core

### Packages

In the previous EF6 example, the main package was installed with:

```powershell
dotnet add package EntityFramework
```

This EF Core project references three packages:

| Package | Purpose in this project |
| --- | --- |
| `Microsoft.EntityFrameworkCore` | Core EF APIs, including `DbContext` |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server support, including `UseSqlServer` |
| `Microsoft.EntityFrameworkCore.Tools` | Visual Studio Package Manager Console migration commands |

The following commands reproduce the versions declared in the supplied `.csproj`:

```powershell
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.12
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.12
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.12
```

> **Extra clarification:** Three direct package references are the setup used here, not a universal minimum for every EF Core application. The SQL Server provider brings core dependencies, and migration tooling is needed for development tasks. `Microsoft.EntityFrameworkCore.Tools` supplies the Visual Studio console commands; the `dotnet ef` tool has a separate setup. The migration examples below use Visual Studio's Package Manager Console. [Microsoft: Package Manager Console tools](https://learn.microsoft.com/en-us/ef/core/cli/powershell)

### The context

`Context` inherits from `DbContext` and provides the application's entry point for EF operations:

```csharp
internal class Context : DbContext
{
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Branch> Branches { get; set; }
}
```

For example, `context.Departments` is the starting point for the department queries in `Program.cs`.

Two methods are central to this lesson:

| Method | Question it answers |
| --- | --- |
| `OnConfiguring` | Which provider and connection settings should this context use? |
| `OnModelCreating` | How should the entities, properties, keys, and relationships be mapped? |

---

## 3. Defining the connection string

### `OnConfiguring`

Override the method inherited from `DbContext` and configure the SQL Server provider:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(
        @"Data Source=localhost\SQLEXPRESS;Initial Catalog=Intake46Core;Integrated Security=True;TrustServerCertificate=True;");

    base.OnConfiguring(optionsBuilder);
}
```

This uses the same settings as the supplied `Context.cs`, with spacing normalized for readability.

| Setting | Meaning |
| --- | --- |
| `Data Source=localhost\SQLEXPRESS` | Connect to the local SQL Express instance |
| `Initial Catalog=Intake46Core` | Use the `Intake46Core` database |
| `Integrated Security=True` | Use Windows authentication |
| `TrustServerCertificate=True` | Bypass server-certificate validation for this local connection |

`UseSqlServer` comes from the SQL Server provider. The examples also import:

```csharp
using Microsoft.EntityFrameworkCore;
```

### What is `DbContextOptionsBuilder`?

It is the object used to build the context's options. In this lesson, those options specify the provider and connection string.

The mentor connects this API to the **Builder pattern**: options are assembled through method calls. Keep the purpose in mind; a class name ending in `Builder` alone is not enough to prove how a design pattern is implemented.

> **Extra clarification:** Overriding `OnConfiguring` is the approach used in this console application. It is not the only way to configure a context; externally supplied options are also possible. Those alternatives are outside this video's implementation.

### Connection-string troubleshooting

The earlier project discussion encountered these two issues:

| Symptom | What to check |
| --- | --- |
| An SSL/certificate-trust error | The local example includes `TrustServerCertificate=True` |
| `Format of the initialization string does not conform to specification...` | Check `key=value` formatting and the `;` between settings |

For example, `Integrated Security=True;TrustServerCertificate=True;` contains two separate settings. A missing separator can cause parsing to fail.

These are connection-setting changes. They do not change the entity model and do not require a new migration. Save, build, then retry `Update-Database`.

---

## 4. Migrations and the model snapshot

### What does a migration do?

A migration describes a change to the database schema. Its two methods have different directions:

| Method | Purpose |
| --- | --- |
| `Up` | Apply the migration's schema changes |
| `Down` | Reverse those changes when rolling back |

The supplied `init` migration creates `Attendance`, `Branch`, `Department`, and `Employees`. Later migrations add the remaining relationships and `Project`.

### Model, snapshot, and history are different things

| Item | Location | Purpose |
| --- | --- | --- |
| Current model | Entities and context configuration | Describes the model the application currently uses |
| `ContextModelSnapshot.cs` | Project's `Migrations` folder | Represents the model after the latest scaffolded migration |
| `__EFMigrationsHistory` | Database | Records applied migration IDs and EF product versions |

Adding a migration compares the current model with the previous snapshot. Updating the database applies pending migration operations and records their application. [Microsoft: migrations overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

```text
Change entities or mapping
          |
          v
Add-Migration
  Compare current model with snapshot
  Generate migration files and update snapshot
          |
          v
Update-Database
  Apply pending migration operations
  Record applied migrations in database history
```

> **Correction:** EF Core does not keep “two model snapshots,” one in Visual Studio and one in the database. Its history table records applied migrations; it does not store a second complete model snapshot. Also, scaffolding a migration does not inspect every live database table to discover manual schema changes.

> **EF6 clarification:** EF6 also has model metadata in migration files and stores model information in its database history. Its change detection is not simply “compare current code directly with live database tables.” EF Core's separate snapshot file is the important distinction here. [Microsoft: EF6 migrations with an existing database](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/existing-database)

### Why use `Remove-Migration` instead of deleting a file?

`Add-Migration` updates the snapshot immediately, before `Update-Database`.

If a newly added property is already represented in the snapshot, deleting only its migration file leaves an inconsistent migration history. A later migration may not generate the missing operation because that property is already in the snapshot.

Use `Remove-Migration` to remove the latest migration and restore the preceding snapshot state. Removing the only remaining migration also removes the snapshot. It does **not** undo edits to the entity classes. [Microsoft: managing migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)

### If the migration was already applied

For the local learning database, roll back to the previous migration before removing the latest one:

```powershell
Update-Database ProjectClassAndRelation
Remove-Migration
```

This example assumes the latest applied migration is `RelationEmpAttend`, immediately after `ProjectClassAndRelation`, as in the supplied files. Rollback executes the relevant `Down` operations, which can remove schema or data.

### Existing database: why did the mentor empty `Up` and `Down`?

The lecture demonstrates a special case: the tables already exist, but the migration baseline needs to be recreated.

If the existing schema **already matches the current model**, an empty baseline migration can capture the model without trying to recreate the existing tables:

1. Generate the baseline migration and snapshot.
2. Review the existing schema against the model.
3. Empty the baseline's `Up` and `Down` operations.
4. Apply the baseline to record it in migration history.

This explains the demonstration, not a general fix whenever a database exists. Normal `Update-Database` already supports an existing database with valid migration history. An empty baseline cannot create those tables in a fresh database and will not repair a schema mismatch.

---

## 5. Important commands till now

Run these in **Visual Studio → Package Manager Console**, with the EF Core project selected as the default project and the appropriate startup project selected.

| Command | Meaning |
| --- | --- |
| `Add-Migration init` | Scaffold a migration named `init` |
| `Update-Database` | Apply pending migrations |
| `Remove-Migration` | Remove the latest unapplied migration and restore its snapshot state |
| `Update-Database PreviousMigrationName` | Return the database to a specified migration |

EF Core has no `Enable-Migrations` step. Installing the tools makes the console commands available; `Add-Migration` creates the initial migration files.

The supplied project already contains these migrations, in order:

```text
init
RelationBranchDept
ProjectClassAndRelation
RelationEmpAttend
```

For this existing project, use `Update-Database` to apply its migrations. Do not add another `init` just to run it.

A normal change follows this sequence:

```text
Edit model -> Save/build -> Add-Migration -> Review Up/Down -> Update-Database
```

---

## 6. Fluent API

### How to use Fluent API

Override `OnModelCreating` and use its `ModelBuilder` to configure the model:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Department>().ToTable("Department");
    modelBuilder.Entity<Department>().Property(d => d.Name).IsRequired(true);
    modelBuilder.Entity<Attendance>().HasKey(k => new { k.EmployeeID, k.Date });

    base.OnModelCreating(modelBuilder);
}
```

This is the configuration used in `Context.cs`.

Read each chain as an instruction:

```text
Entity<Department>()   -> Select the Department entity
ToTable("Department") -> Map it to the Department table

Property(d => d.Name)  -> Select its Name property
IsRequired(true)      -> Make that property required
```

> **Correction:** The original code comment beside `ToTable` says “change column name.” `ToTable` changes the **table mapping**, not a column name.

**Extra clarification — column mapping example:** To change a column name, the corresponding API would be:

```csharp
modelBuilder.Entity<Department>()
    .Property(d => d.Name)
    .HasColumnName("DepartmentName");
```

This example is not configured in the supplied project. [Microsoft: entity properties](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties)

---

## 7. Changing defaults and configuration by convention

There are three ways to describe the model:

| Approach | How it works | Project example |
| --- | --- | --- |
| Convention | EF infers mapping from supported patterns | `ID` becomes a primary key |
| Data Annotations | Attributes on an entity or property | `[Table("Branch")]` |
| Fluent API | Configuration in `OnModelCreating` | `HasKey(...)` |

Convention means following a default rule. To replace a default mapping, use an annotation or Fluent API. When configurations conflict, Fluent API takes precedence over annotations, and annotations over conventions. [Microsoft: creating and configuring a model](https://learn.microsoft.com/en-us/ef/core/modeling/)

### Primary-key and naming conventions

Properties named `ID` or the entity name followed by `ID`, such as `EmployeeID` on an `Employee`, can be recognized as primary keys. In this SQL Server project, the single integer `ID` keys are generated as identity columns. [Microsoft: keys](https://learn.microsoft.com/en-us/ef/core/modeling/keys)

**Extra clarification:** Table naming and column naming are separate. EF Core normally uses the `DbSet` property name for a table, or the entity name when there is no `DbSet`. Scalar properties normally retain their names as column names. Explicit table mappings override the default. [Microsoft: entity types](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types)

The supplied snapshot confirms:

| Entity | Table | Reason |
| --- | --- | --- |
| `Branch` | `Branch` | `[Table("Branch")]` |
| `Department` | `Department` | `ToTable("Department")` |
| `Employee` | `Employees` | `DbSet<Employee> Employees` |
| `Project` | `Project` | Entity name; no dedicated `DbSet` |
| `Attendance` | `Attendance` | Entity name; no dedicated `DbSet` |

### Foreign-key convention: Employee and Department

From `Employee.cs`:

```csharp
public int DepartmentID { get; set; }
public Department Department { get; set; }
```

From `Department.cs`:

```csharp
public ICollection<Employee> Employees { get; set; }
```

The navigation properties describe the relationship. EF recognizes `DepartmentID` as its foreign key by convention, so this example does not need a `[ForeignKey]` attribute. The supplied `init` migration creates the FK and its index.

> **Extra clarification:** An integer named `DepartmentID` alone does not describe the complete relationship. Relationships may be inferred from navigations or configured explicitly. If navigations establish a relationship without a CLR FK property, EF can use a shadow FK. EF6 also has relationship conventions; FK attributes were needed for the earlier example, not for every EF6 relationship. [Microsoft: relationship discovery conventions](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/conventions)

### Question: when do I use an object and when do I use `ICollection<T>`?

Use a reference navigation when that entity points to **one** related entity. Use a collection navigation when it points to **many** related entities.

```text
One Employee belongs to one Department:
Employee.Department -> Department

One Department has many Employees:
Department.Employees -> ICollection<Employee>
```

`DepartmentID` stores the related row's key. `Department` provides access to the related entity in C#. `Employees` provides access to the related collection.

Declaring a navigation describes the model; it does not guarantee that related data has been loaded.

---

## 8. Composite key

`Attendance` contains:

```csharp
public int EmployeeID { get; set; }
public DateTime Date { get; set; }
public Employee Employee { get; set; }
```

An attendance record is identified by the **combination** of employee and date:

```csharp
modelBuilder.Entity<Attendance>()
    .HasKey(k => new { k.EmployeeID, k.Date });
```

`new { ... }` is an anonymous object expression that supplies the two key properties. It does not create two independent primary keys.

| EmployeeID | Date | Result |
| --- | --- | --- |
| 1 | 2000-01-01 00:00:00 | One record |
| 1 | 2000-01-02 00:00:00 | Valid: different date |
| 2 | 2000-01-01 00:00:00 | Valid: different employee |
| 1 | 2000-01-01 00:00:00 | Duplicate combination |

The initial migration confirms the generated constraint:

```csharp
table.PrimaryKey("PK_Attendance", x => new { x.EmployeeID, x.Date });
```

**Extra clarification:** `Date` is a `DateTime`, and the migration maps it to `datetime2`. The time component participates in uniqueness. The current key does not independently enforce one record per calendar day when different times are supplied.

> **Correction and version note:** In EF6, composite-key annotations require ordering as well as `[Key]`. Applying `[Key]` to multiple properties is not the EF Core solution. The video uses Fluent API, and so does this project. Modern EF Core also supports a class-level `[PrimaryKey(...)]` attribute, so “Fluent API is the only possible way” is not accurate for the supplied EF Core version. [Microsoft: keys](https://learn.microsoft.com/en-us/ef/core/modeling/keys)

### Why does Attendance appear without a `DbSet`?

`modelBuilder.Entity<Attendance>()` explicitly includes it in the model. Entity types can also be discovered through navigations, which is how `Project` is reached from `Department.Projects`. A separate `DbSet` is not required for every mapped entity. [Microsoft: entity types](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types)

---

## 9. Revision on Data Annotations

### Attributes actually used in this project

From `Branch.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Branch")]
internal class Branch
{
    public int ID { get; set; }

    [Required]
    public string Name { get; set; }

    public ICollection<Department> Departments { get; set; }
}
```

| Attribute | Meaning for this model |
| --- | --- |
| `[Table("Branch")]` | Map the entity to the `Branch` table |
| `[Required]` | Configure `Name` as required |

The lecture also recalls maximum-length annotations and the earlier lesson's key/FK annotations. Those are revision topics, not additional attributes currently configured in these entity files.

### Required mapping vs. validation

> **Correction:** EF Core uses annotations to configure its model, but it does not automatically perform EF6-style entity validation during `SaveChanges`. Attributes can also be used by a separate validation system; this console application does not demonstrate one. A database constraint failure during a save can surface as `DbUpdateException` with a provider exception inside it. Do not assume `[Required]` alone provides a complete validation flow. [Microsoft: detailed EF6-to-EF-Core differences](https://learn.microsoft.com/ef/efcore-and-ef6/porting/port-detailed-cases)

### Version note: nullable reference types

The supplied `.csproj` enables:

```xml
<Nullable>enable</Nullable>
```

With this setting, `string Name` is required by convention, while `string? Name` would be optional unless overridden. This helps explain why the supplied snapshot also marks `Employee.Name` and `Project.Name` as required despite neither property having `[Required]`. Uninitialized non-nullable properties can still produce compiler warnings; declaring `string` does not initialize it. [Microsoft: required and optional properties](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties#required-and-optional-properties)

---

## 10. Rest of the relationships

All four relationships in the completed example are one-to-many:

```text
Branch
  └── many Departments
        ├── many Projects
        └── many Employees
              └── many Attendance records
```

| One side | Many side | Reference navigation on many side | Collection on one side | FK |
| --- | --- | --- | --- | --- |
| `Branch` | `Department` | `Department.Branch` | `Branch.Departments` | `Department.BranchID` |
| `Department` | `Employee` | `Employee.Department` | `Department.Employees` | `Employee.DepartmentID` |
| `Department` | `Project` | `Project.department` | `Department.Projects` | `Project.DepartmentID` |
| `Employee` | `Attendance` | `Attendance.Employee` | `Employee.Attendances` | `Attendance.EmployeeID` |

### Branch → Department

```csharp
// Branch.cs
public ICollection<Department> Departments { get; set; }

// Department.cs
public int BranchID { get; set; }
public Branch Branch { get; set; }
```

### Department → Project

```csharp
// Department.cs
public ICollection<Project> Projects { get; set; }

// Project.cs
public int DepartmentID { get; set; }
public Department department { get; set; }
```

The lowercase `department` is preserved from the supplied code.

### Employee → Attendance

```csharp
// Employee.cs
public ICollection<Attendance> Attendances { get; set; }

// Attendance.cs
public int EmployeeID { get; set; }
public Employee Employee { get; set; }
```

`Attendance.EmployeeID` has two roles: it is part of the composite primary key and the foreign key to `Employee`.

The supplied snapshot confirms all four relationships, required FKs, and cascade-delete mappings. These are observations from the generated project model; cascade behavior is not explored further in this video.

---

## 11. What happens when the model and database differ?

Changing a C# entity does not automatically change the database. Likewise, manually changing a SQL table does not automatically update the C# entities or snapshot.

### EF6

The familiar EF6 Code First workflow can throw an `InvalidOperationException` saying that the model backing the context has changed since the database was created. This occurs during database initialization/use when the relevant compatibility check runs, rather than necessarily at the application's first line. [Microsoft: EF6 Code First migrations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/)

### EF Core

As the lecture explains, ordinary EF Core querying does not provide that same blanket EF6-style startup check. A mismatch can become visible when SQL needs the missing or incompatible schema element.

| Example | When the problem can surface |
| --- | --- |
| An entity maps `Salary`, but its SQL column is missing | Executing SQL that selects or writes `Salary` |
| A mapped table is missing | Executing a query against that table |
| An FK value points to a nonexistent parent | Saving changes when the database checks the FK |
| A migration tries to recreate an existing table | Applying that migration |

> **Correction:** You do not have to explicitly read `employee.Salary` in C# to trigger an invalid-column error. Loading a complete entity can already select all its mapped columns. A SQL Server query can throw a `SqlException`; save failures commonly surface as `DbUpdateException`. The exact exception depends on the failing operation.

In `Program.cs`, constructing the LINQ query describes the work. Enumerating it in `foreach` executes the query, so a schema-related query error can appear there.

**Version clarification:** Modern migration application can also reject pending model changes. That is distinct from comparing every live database object at ordinary query startup. [Microsoft: managing migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)

The practical workflow is to update the model, generate and review a migration, then apply it. Inspect unexpected manual database changes separately.

---

## 12. Lazy loading and loading related data

### Lazy loading is not enabled by default

The starting query in the supplied `Program.cs` is commented out:

```csharp
var query =
    from d in context.Departments
    where d.ID < 4
    select d;
```

This selects departments. It does not request their employees.

The following loop therefore must not assume `Employees` has been loaded:

```csharp
foreach (var dept in query)
{
    foreach (var emp in dept.Employees)
    {
        Console.WriteLine(emp.Name);
    }

    Console.WriteLine(dept.Name);
}
```

In the lecture's fresh-context example, the uninitialized collection is `null`, producing a `NullReferenceException`. Accessing it does not automatically load employees.

> **Extra clarification:** An unloaded collection is not guaranteed to be `null` in every application. It might be initialized or populated from entities already tracked by the context. An empty collection alone is not proof that a department has no employees in the database. Initializing the collection can prevent a null iteration but does not load its rows. [Microsoft: eager loading and navigation fix-up](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)

### Why does the lecture discuss performance here?

The mentor uses UI data binding to explain hidden database access: a control may access navigation properties, which can trigger additional queries when lazy loading is enabled. Nested relationships can load much more data than the developer intended.

Understand the loading decision before choosing the syntax: **which related data does this operation actually need?**

### Four options mentioned in the transcript

| Option | Basic idea | Coverage in this supplied video |
| --- | --- | --- |
| Eager loading | Request related entities with the initial query | Demonstrated with `Include` and `ThenInclude` |
| Explicit loading | Deliberately load a navigation later | Named; not implemented |
| “Select loading” / projection | Select the shape of data needed | Named; not implemented |
| Lazy loading | Load related data when a navigation is accessed | Default behavior discussed; setup not demonstrated |

“Select loading” is the lecture's terminology. Projection is a query-shaping approach rather than a fourth navigation-loading mechanism. The practical examples below stay with the eager loading actually demonstrated.

### Eager loading: `Include`

To load departments with their employees:

```csharp
var query = context.Departments
    .Include(d => d.Employees);
```

The query requests employees as part of loading departments. It executes when enumerated.

### Two collections on the same entity

To load both collections belonging to `Department`:

```csharp
var query = context.Departments
    .Include(d => d.Projects)
    .Include(d => d.Employees);
```

Both `Include` calls start from the root entity, `Department`.

### Nested related data: `ThenInclude`

The active query in the supplied `Program.cs` is:

```csharp
var query = context.Departments
    .Include(d => d.Projects)
    .Include(d => d.Employees)
    .ThenInclude(e => e.Attendances);
```

Read its paths separately:

```text
Department -> Projects
Department -> Employees -> Attendances
```

| Call | Where it starts | What it requests |
| --- | --- | --- |
| `Include(d => d.Projects)` | Department | Its projects |
| `Include(d => d.Employees)` | Department | Its employees |
| `ThenInclude(e => e.Attendances)` | Each employee from the preceding path | That employee's attendance records |

Use `Include` for another path from the root. Use `ThenInclude` to continue the preceding path. `ThenInclude` does not have to be the final call in a query.

The query's root results are still departments. Including employees and attendance records does not turn the query into an employee or attendance query. [Microsoft: eager loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)

### What does the current program display?

It prints employee names followed by each department's name. Projects and attendance records are loaded by the query but are not printed by that loop.

> **Extra clarification — performance preview:** Requesting multiple collections can increase the data returned through joins. Eager loading is not automatically the best choice for every screen or query. The transcript ends just as the mentor introduces this tradeoff, so its detailed solutions belong to the next video. [Microsoft: eager-loading considerations](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)

> **Extra clarification — lazy-loading setup:** Adding `virtual` alone does not enable EF Core lazy loading. Proxy-based loading requires the proxies package, configuration, and suitable entity/navigation declarations; another approach uses `ILazyLoader`. Neither is configured in the supplied project. [Microsoft: lazy loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy)

---

## 13. EF6 vs. EF Core comparison

This comparison covers the topics raised in my notes and this first video. It is not an exhaustive feature list.

| Topic | EF6 | EF Core / this project |
| --- | --- | --- |
| Library identity | Entity Framework 6 | Entity Framework Core; distinct from the .NET version |
| Database providers | SQL Server and other providers | Provider packages; this project uses SQL Server |
| Package setup shown | `EntityFramework` | Core, SQL Server, and Tools references |
| Provider/connection configuration | Earlier example used a context base constructor | This example overrides `OnConfiguring` and calls `UseSqlServer` |
| Enable migrations | `Enable-Migrations` in the Code First workflow | No equivalent enable command |
| Migration model metadata | Metadata in migrations and database history | Separate `ContextModelSnapshot.cs`; applied IDs in `__EFMigrationsHistory` |
| Removing the latest migration | Earlier lesson discussed deleting unapplied files | Use `Remove-Migration` to keep the snapshot consistent |
| Configuration approaches | Conventions, annotations, Fluent API | Same three approaches; individual rules/APIs can differ |
| FK conventions | Available; annotations are not universally required | The project relationships are inferred from navigations and FK names |
| Composite-key annotations | `[Key]` with ordering | Project uses `HasKey`; modern versions also support `[PrimaryKey]` |
| Entity validation on save | Built-in validation in the EF6 workflow | No equivalent automatic Data Annotation validation |
| Lazy loading | Available when its requirements are met | Opt-in; this project uses eager loading |
| Model/database mismatch | A Code First initializer can detect a model mismatch | Ordinary query failures can appear when incompatible SQL executes |
| Performance | Depends on workload and query design | Improvements do not make every EF Core query automatically faster |

Corrections in this table are explained and sourced in their corresponding sections above. The key comparison is behavioral: understand what changed, what stayed familiar, and which settings this project actually uses.

---

## 14. Revision questions

| Question | Answer |
| --- | --- |
| Where do I select the provider in this project? | `OnConfiguring`, using `UseSqlServer` |
| Where do I configure mappings and keys? | `OnModelCreating`, using `ModelBuilder` |
| Does `Add-Migration` update the database? | No; it generates files and updates the snapshot |
| What applies pending migrations? | `Update-Database` |
| Why should I avoid manually deleting a migration file? | Its changes may already be represented in the snapshot |
| Does `Remove-Migration` remove the property I added to my entity? | No; model-code edits are separate |
| Is the history table another complete model snapshot? | No; it records applied migrations and product versions |
| Does `ToTable` rename a column? | No; it sets the table mapping |
| Object or `ICollection<T>`? | Reference for one related entity; collection for many |
| What uniquely identifies Attendance? | The combination of `EmployeeID` and `Date` |
| Must every entity have its own `DbSet`? | No; explicit model configuration or navigation discovery can include it |
| Does a navigation automatically load its data? | No |
| What is the difference between `Include` and `ThenInclude`? | Root path vs. continuation of the preceding related path |
| Does the active program print projects and attendance? | No; it loads them but prints employee and department names |
| What remains for the second video? | The detailed loading tradeoffs and further techniques not demonstrated before the break |

### Before running the supplied example

1. Use an SDK compatible with the declared `net10.0` target and restore the declared packages.
2. Check that the local SQL Server instance and authentication match `Context.cs`.
3. Build the project.
4. Run `Update-Database` in Visual Studio's Package Manager Console to apply the existing migrations.
5. Provide related sample rows if you want visible query output; the lecture entered its demonstration data through the database UI.
6. Run the console application and trace each `Include` path before looking at the output.

The README records the inspected code and migrations. It does not assert that the local SQL Server database was connected to or modified during preparation.
