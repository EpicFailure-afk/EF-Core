# EF Core — Study Notes: Video 1 & Video 2

A concept-first reference for both EF Core videos, following the order of my original notes and using the entities, configuration, queries, and migrations from my project. Start with the problem, understand the idea, then read the code.

**Repository:** [EpicFailure-afk/EF-Core](https://github.com/EpicFailure-afk/EF-Core)

**Video 1:** configuration, migrations, Fluent API, conventions, Data Annotations, relationships, and the introduction to eager loading.

**Video 2:** loading tradeoffs, client/server evaluation, tracking and identity resolution, global query filters, shadow properties, and the self-referencing relationship exercise suggested at the end of the lecture.

> **How to read these notes**
> The main explanations come from my notes, the transcript, and the supplied local code. **Correction**, **Version note**, and **Extra clarification** identify additions that resolve an inaccurate statement or explain behavior beyond the demonstrated example. Microsoft documentation is linked beside those additions. The GitHub page could not be retrieved during preparation; code and migration details were checked against the supplied local repository.

## Contents

- [Video 1 — Foundations](#video-1--foundations)
- [Video 2 — Loading, Tracking, and Model Features](#video-2--loading-tracking-and-model-features)

### Video 1 topics

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

### Video 2 topics

1. [Read the examples without mixing their roles](#v2-1-read-the-examples-without-mixing-their-roles)
2. [Eager loading: single and split queries](#v2-2-eager-loading-single-and-split-queries)
3. [Explicit loading](#v2-3-explicit-loading)
4. [Select loading: projection](#v2-4-select-loading-projection)
5. [Lazy loading](#v2-5-lazy-loading)
6. [Client evaluation and server evaluation](#v2-6-client-evaluation-and-server-evaluation)
7. [Using EF.Functions](#v2-7-using-effunctions)
8. [Tracking and identity resolution](#v2-8-tracking-and-identity-resolution)
9. [Global query filters](#v2-9-global-query-filters)
10. [Shadow properties](#v2-10-shadow-properties)
11. [Applying properties to all entities](#v2-11-applying-properties-to-all-entities)
12. [Recursive relationships and relationship fixup](#v2-12-recursive-relationships-and-relationship-fixup)
13. [What is currently active in my project?](#v2-13-what-is-currently-active-in-my-project)
14. [Revision questions and decisions](#v2-14-revision-questions-and-decisions)

---

## Video 1 — Foundations

This part describes the project as it stood at the end of the first video. References to the active printing loop and its query below refer to that version; Video 2 records the updated code separately.

---

### 1. Versions: EF6 and EF Core

First, separate the runtime from the data-access library:

| Name | What it represents |
| --- | --- |
| .NET Framework / .NET | The platform used to build and run the application |
| EF6 / EF Core | The library used to map entities and work with a database |

The video introduces the move from .NET Core 3.1 to the .NET 5 naming, then uses .NET 5 for its examples. My supplied project targets `net10.0` and references EF Core packages at version `10.0.12`.

**EF Core is still called EF Core.** It does not become “EF6” when an application uses .NET 6.

#### Database providers

A **provider** lets EF work with a particular database. This project uses SQL Server, so it uses the SQL Server provider. The lecture also mentions Oracle, SQLite, and the non-relational Azure Cosmos DB provider as examples of other choices.

> **Correction:** EF6 is not limited to SQL Server. Other EF6 providers exist, including third-party providers. “We used SQL Server with EF6” describes the earlier course example, not the full capabilities of EF6. [Microsoft: EF6 providers](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/providers/)

The useful lesson is to identify both the EF library and the provider the application needs.

---

### 2. How to configure EF Core

#### Packages

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

#### The context

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

### 3. Defining the connection string

#### `OnConfiguring`

Override the method inherited from `DbContext` and configure the SQL Server provider:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(
        @"Data Source=localhost\SQLEXPRESS;Initial Catalog=Intake46Core;
        Integrated Security=True;TrustServerCertificate=True;");

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

#### What is `DbContextOptionsBuilder`?

It is the object used to build the context's options. In this lesson, those options specify the provider and connection string.

The mentor connects this API to the **Builder pattern**: options are assembled through method calls. Keep the purpose in mind; a class name ending in `Builder` alone is not enough to prove how a design pattern is implemented.

> **Extra clarification:** Overriding `OnConfiguring` is the approach used in this console application. It is not the only way to configure a context; externally supplied options are also possible. Those alternatives are outside this video's implementation.

#### Connection-string troubleshooting

The earlier project discussion encountered these two issues:

| Symptom | What to check |
| --- | --- |
| An SSL/certificate-trust error | The local example includes `TrustServerCertificate=True` |
| `Format of the initialization string does not conform to specification...` | Check `key=value` formatting and the `;` between settings |

For example, `Integrated Security=True;TrustServerCertificate=True;` contains two separate settings. A missing separator can cause parsing to fail.

These are connection-setting changes. They do not change the entity model and do not require a new migration. Save, build, then retry `Update-Database`.

---

### 4. Migrations and the model snapshot

#### What does a migration do?

A migration describes a change to the database schema. Its two methods have different directions:

| Method | Purpose |
| --- | --- |
| `Up` | Apply the migration's schema changes |
| `Down` | Reverse those changes when rolling back |

The supplied `init` migration creates `Attendance`, `Branch`, `Department`, and `Employees`. Later migrations add the remaining relationships and `Project`.

#### Model, snapshot, and history are different things

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

#### Why use `Remove-Migration` instead of deleting a file?

`Add-Migration` updates the snapshot immediately, before `Update-Database`.

If a newly added property is already represented in the snapshot, deleting only its migration file leaves an inconsistent migration history. A later migration may not generate the missing operation because that property is already in the snapshot.

Use `Remove-Migration` to remove the latest migration and restore the preceding snapshot state. Removing the only remaining migration also removes the snapshot. It does **not** undo edits to the entity classes. [Microsoft: managing migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)

#### If the migration was already applied

For the local learning database, roll back to the previous migration before removing the latest one:

```powershell
Update-Database ProjectClassAndRelation
Remove-Migration
```

This example assumes the latest applied migration is `RelationEmpAttend`, immediately after `ProjectClassAndRelation`, as in the supplied files. Rollback executes the relevant `Down` operations, which can remove schema or data.

#### Existing database: why did the mentor empty `Up` and `Down`?

The lecture demonstrates a special case: the tables already exist, but the migration baseline needs to be recreated.

If the existing schema **already matches the current model**, an empty baseline migration can capture the model without trying to recreate the existing tables:

1. Generate the baseline migration and snapshot.
2. Review the existing schema against the model.
3. Empty the baseline's `Up` and `Down` operations.
4. Apply the baseline to record it in migration history.

This explains the demonstration, not a general fix whenever a database exists. Normal `Update-Database` already supports an existing database with valid migration history. An empty baseline cannot create those tables in a fresh database and will not repair a schema mismatch.

---

### 5. Important commands till now

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

### 6. Fluent API

#### How to use Fluent API

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

### 7. Changing defaults and configuration by convention

There are three ways to describe the model:

| Approach | How it works | Project example |
| --- | --- | --- |
| Convention | EF infers mapping from supported patterns | `ID` becomes a primary key |
| Data Annotations | Attributes on an entity or property | `[Table("Branch")]` |
| Fluent API | Configuration in `OnModelCreating` | `HasKey(...)` |

Convention means following a default rule. To replace a default mapping, use an annotation or Fluent API. When configurations conflict, Fluent API takes precedence over annotations, and annotations over conventions. [Microsoft: creating and configuring a model](https://learn.microsoft.com/en-us/ef/core/modeling/)

#### Primary-key and naming conventions

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

#### Foreign-key convention: Employee and Department

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

#### Question: when do I use an object and when do I use `ICollection<T>`?

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

### 8. Composite key

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

#### Why does Attendance appear without a `DbSet`?

`modelBuilder.Entity<Attendance>()` explicitly includes it in the model. Entity types can also be discovered through navigations, which is how `Project` is reached from `Department.Projects`. A separate `DbSet` is not required for every mapped entity. [Microsoft: entity types](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types)

---

### 9. Revision on Data Annotations

#### Attributes actually used in this project

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

#### Required mapping vs. validation

> **Correction:** EF Core uses annotations to configure its model, but it does not automatically perform EF6-style entity validation during `SaveChanges`. Attributes can also be used by a separate validation system; this console application does not demonstrate one. A database constraint failure during a save can surface as `DbUpdateException` with a provider exception inside it. Do not assume `[Required]` alone provides a complete validation flow. [Microsoft: detailed EF6-to-EF-Core differences](https://learn.microsoft.com/ef/efcore-and-ef6/porting/port-detailed-cases)

#### Version note: nullable reference types

The supplied `.csproj` enables:

```xml
<Nullable>enable</Nullable>
```

With this setting, `string Name` is required by convention, while `string? Name` would be optional unless overridden. This helps explain why the supplied snapshot also marks `Employee.Name` and `Project.Name` as required despite neither property having `[Required]`. Uninitialized non-nullable properties can still produce compiler warnings; declaring `string` does not initialize it. [Microsoft: required and optional properties](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties#required-and-optional-properties)

---

### 10. Rest of the relationships

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

#### Branch → Department

```csharp
// Branch.cs
public ICollection<Department> Departments { get; set; }

// Department.cs
public int BranchID { get; set; }
public Branch Branch { get; set; }
```

#### Department → Project

```csharp
// Department.cs
public ICollection<Project> Projects { get; set; }

// Project.cs
public int DepartmentID { get; set; }
public Department department { get; set; }
```

The lowercase `department` is preserved from the supplied code.

#### Employee → Attendance

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

### 11. What happens when the model and database differ?

Changing a C# entity does not automatically change the database. Likewise, manually changing a SQL table does not automatically update the C# entities or snapshot.

#### EF6

The familiar EF6 Code First workflow can throw an `InvalidOperationException` saying that the model backing the context has changed since the database was created. This occurs during database initialization/use when the relevant compatibility check runs, rather than necessarily at the application's first line. [Microsoft: EF6 Code First migrations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/)

#### EF Core

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

### 12. Lazy loading and loading related data

#### Lazy loading is not enabled by default

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

#### Why does the lecture discuss performance here?

The mentor uses UI data binding to explain hidden database access: a control may access navigation properties, which can trigger additional queries when lazy loading is enabled. Nested relationships can load much more data than the developer intended.

Understand the loading decision before choosing the syntax: **which related data does this operation actually need?**

#### Four options mentioned in the transcript

| Option | Basic idea | Coverage in this supplied video |
| --- | --- | --- |
| Eager loading | Request related entities with the initial query | Demonstrated with `Include` and `ThenInclude` |
| Explicit loading | Deliberately load a navigation later | Named; not implemented |
| “Select loading” / projection | Select the shape of data needed | Named; not implemented |
| Lazy loading | Load related data when a navigation is accessed | Default behavior discussed; setup not demonstrated |

“Select loading” is the lecture's terminology. Projection is a query-shaping approach rather than a fourth navigation-loading mechanism. The practical examples below stay with the eager loading actually demonstrated.

#### Eager loading: `Include`

To load departments with their employees:

```csharp
var query = context.Departments
    .Include(d => d.Employees);
```

The query requests employees as part of loading departments. It executes when enumerated.

#### Two collections on the same entity

To load both collections belonging to `Department`:

```csharp
var query = context.Departments
    .Include(d => d.Projects)
    .Include(d => d.Employees);
```

Both `Include` calls start from the root entity, `Department`.

#### Nested related data: `ThenInclude`

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

#### What does the current program display?

It prints employee names followed by each department's name. Projects and attendance records are loaded by the query but are not printed by that loop.

> **Extra clarification — performance preview:** Requesting multiple collections can increase the data returned through joins. Eager loading is not automatically the best choice for every screen or query. The transcript ends just as the mentor introduces this tradeoff, so its detailed solutions belong to the next video. [Microsoft: eager-loading considerations](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)

> **Extra clarification — lazy-loading setup:** Adding `virtual` alone does not enable EF Core lazy loading. Proxy-based loading requires the proxies package, configuration, and suitable entity/navigation declarations; another approach uses `ILazyLoader`. Neither is configured in the supplied project. [Microsoft: lazy loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy)

---

### 13. EF6 vs. EF Core comparison

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

### 14. Revision questions

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

#### Before running the supplied example

1. Use an SDK compatible with the declared `net10.0` target and restore the declared packages.
2. Check that the local SQL Server instance and authentication match `Context.cs`.
3. Build the project.
4. Run `Update-Database` in Visual Studio's Package Manager Console to apply the existing migrations.
5. Provide related sample rows if you want visible query output; the lecture entered its demonstration data through the database UI.
6. Run the console application and trace each `Include` path before looking at the output.

The README records the inspected code and migrations. It does not assert that the local SQL Server database was connected to or modified during preparation.

---

## Video 2 — Loading, Tracking, and Model Features

The first video built the model and introduced `Include`. The second asks what happens behind those calls: which SQL is sent, when it executes, which objects EF creates, and how model configuration affects queries.

Examples labeled **From my code** preserve the supplied example. **Correction** explains a discrepancy without changing the source files. **Extra example** supplies a missing demonstration or completes an unfinished note; it is not presented as code copied from the lecture.

### V2-1. Read the examples without mixing their roles

Before choosing an EF method, ask what decision you are making:

| Decision | Question | Relevant APIs |
| --- | --- | --- |
| Execution | When does the database work happen? | `ToList`, `First`, enumeration |
| Loading | Which related data do I need? | `Include`, `ThenInclude`, `Load`, projection, lazy loading |
| Query splitting | How should included collections be fetched? | `AsSingleQuery`, `AsSplitQuery` |
| Tracking | Should the context track the returned entities? | `AsTracking`, `AsNoTracking` |
| Identity resolution | Should repeated occurrences of a row share an object? | Tracking or `AsNoTrackingWithIdentityResolution` |
| Filtering | Which rows should the query return? | `Where`, `HasQueryFilter` |
| Mapping | Which properties belong to the EF model? | `Property`, shadow-property configuration |

These decisions can work together. `AsSplitQuery` does not turn tracking off. `AsNoTracking` does not select which collections to load. A shadow property does not automatically create a query filter.

#### A query definition is not its result

```csharp
// Extra example, based on the Department queries.
var query = context.Departments.Where(d => d.ID < 4);

// Execute and materialize the results now.
var departments = query.ToList();
```

Think of the first line as a description of the work. The second asks EF to do that work and create a `List<Department>`. A `foreach` over the query would also execute it.

In this lesson, **client** means the C# application and its memory; **server** means the database. It does not mean the end user's browser.

### V2-2. Eager loading: single and split queries

#### The problem introduced before the break

```csharp
// From my code: the eager-loading example is commented out in Program.cs.
var query =
    (context.Departments.AsSingleQuery()
        .Include(d => d.Projects)
        .Include(d => d.Employees)
        .ThenInclude(e => e.Attendances)).ToList();
```

The requested paths are:

```text
Department -> Projects
Department -> Employees -> Attendances
```

One LINQ query can return an object graph, while the SQL result is a flat set of rows. EF reconstructs that graph from the rows.

#### Why can the SQL result become large?

`Projects` and `Employees` are both collections belonging to the same department. Joining those sibling collections can repeat their combinations. The nested attendance path adds detail to the employee side.

**Extra clarification — small example:** Suppose one department has 2 projects and 3 employees, and each employee has 2 attendance records. The employee/attendance path has 6 rows; combining it with 2 projects produces 12 joined rows for that department. Department and project data is repeated in those rows.

> **Correction:** Do not multiply the total row counts of all four tables and assume that is always the returned count. Matching relationships determine the rows. Also, a database does not necessarily physically build the full Cartesian product before filtering a join. The useful concern here is repeated data from sibling collections. [Microsoft: single vs. split queries](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)

#### The intended alternative: `AsSplitQuery`

```csharp
// Correction example: the split-query alternative described in the notes.
// The source example remains unchanged.
var departments = context.Departments
    .AsSplitQuery()
    .Include(d => d.Projects)
    .Include(d => d.Employees)
    .ThenInclude(e => e.Attendances)
    .ToList();
```

| Method | Role |
| --- | --- |
| `AsSingleQuery()` | Request a single SQL query for this loading operation |
| `AsSplitQuery()` | Fetch included collections through multiple SQL queries |
| `ToList()` | Execute the operation and collect its results |

**Remember:** `ToList()` alone does not remove joins. Split queries can still contain joins; splitting is not a promise of join-free SQL.

Split queries can reduce repeated data, but add database round trips and may observe changes between queries. Neither strategy is always faster. [Microsoft: split-query tradeoffs](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries#characteristics-of-split-queries)

#### Seeing the work instead of guessing

The transcript inspects logs using the configuration also present, commented out, in `Context.cs`:

```csharp
optionsBuilder.LogTo(log => Debug.WriteLine(log));
```

For that demonstration, logs appear in Visual Studio's Debug output. Execute the query before expecting its database commands to appear. The current project keeps logging disabled.

### V2-3. Explicit loading

#### The idea

Load the department first. Later, deliberately ask EF to load a particular navigation for that department.

```csharp
// From my code: query_2 executes; the following loop is commented out.
var query_2 = context.Departments.ToList();

foreach (var dept in query_2)
{
    context.Entry(dept).Collection(c => c.Employees).Load();
    context.Entry(dept).Reference(o => o.Branch).Load();
}
```

Read the first loading call from left to right:

```text
Entry(dept)              -> Get EF's entry for this department
Collection(...Employees) -> Select its collection navigation
Load()                  -> Fetch the related employees now
```

Use `Collection` for many related entities and `Reference` for one related entity. The calls above rely on the normal tracking behavior of the example. [Microsoft: explicit loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/explicit)

#### Adding a condition

**From my code:**

```csharp
var emps = context.Entry(dept)
    .Collection(c => c.Employees)
    .Query()
    .Where(e => e.ID < 10);
```

`Query()` gives a related query to compose; `Where` adds a condition. This line alone does not execute it.

**Correction example — executing that filtered query:**

```csharp
var emps = context.Entry(dept)
    .Collection(c => c.Employees)
    .Query()
    .Where(e => e.ID < 10)
    .ToList();

foreach (var emp in emps)
{
    Console.WriteLine(emp.Name);
}
```

In the original loop, `Load()` already loads all visible employees before the filtered query is defined. Defining that query does not remove employees from `dept.Employees`. Use the filtered result when the goal is to print only its matches.

#### What is the cost?

Loading employees separately for every department can produce one initial query plus one employee query per department: the familiar **N+1 pattern**. Loading each branch can add more calls. Explicit loading gives control over timing; it is not automatically a performance improvement.

### V2-4. Select loading: projection

The instructor leaves this topic as an exercise. In these notes, “Select loading” means **projection**: choose the shape of the result using `Select`.

#### The idea

If a screen needs department names and employee names, ask for those values instead of loading every property of every entity.

```csharp
// Extra example: completes the unfinished Select Loading note.
var summaries = context.Departments
    .Select(d => new
    {
        DepartmentName = d.Name,
        EmployeeNames = d.Employees
            .Select(e => e.Name)
            .ToList()
    })
    .ToList();
```

`new { ... }` describes the result shape. This query returns summaries, not complete `Department` entities. The navigation inside the query expression lets EF build the related query; it is not an attempt to iterate an unloaded collection in application memory.

`Include` requests related entities. Projection requests the selected result shape, so this example does not need an `Include` to retrieve employee names.

**Extra clarification:** A projection containing only scalar values has no entity instances to track. A projection that contains an actual entity can still track that entity. “Every `Select` disables tracking” is incorrect. [Microsoft: tracking and custom projections](https://learn.microsoft.com/en-us/ef/core/querying/tracking#tracking-and-custom-projections)

### V2-5. Lazy loading

#### The idea

With lazy loading configured, accessing an unloaded navigation can trigger a database query at that moment.

```text
Load Department
      |
      v
Access department.Employees
      |
      v
Lazy-loading mechanism fetches employees if needed
```

This saves an explicit loading call in the application code, but the query still happens. A loop or UI control can cause unexpected repeated database access.

#### Proxy setup shown in the lecture

The supplied project now references:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Proxies" Version="10.0.12" />
```

The configuration line is present but commented out:

```csharp
optionsBuilder.UseLazyLoadingProxies(true);
```

Navigation properties were changed to `virtual`, for example:

```csharp
public virtual Department Department { get; set; }
public virtual ICollection<Attendance> Attendances { get; set; }
```

> **Extra clarification:** Proxies need entity classes they can inherit from and accessible constructors, as well as overridable navigations. The supplied entities are `internal`; they are not the usual public entity setup shown for proxy loading. Package installation and `virtual` alone do not make this project ready for proxies. No source declarations were changed for this explanation. [Microsoft: lazy-loading proxies](https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy)

The lecture also mentions a more selective approach involving a design pattern but does not implement it. Keep that mention as a future topic, rather than treating “Factory pattern” as the name of a demonstrated per-query EF switch.

**Remember:** These options apply to the configured context. Calling `UseLazyLoadingProxies` in this context does not configure every unrelated context in the entire application.

### V2-6. Client evaluation and server evaluation

#### The problem

A C# method inside a LINQ query is not necessarily something the database provider knows how to translate into SQL.

| Evaluation | Where work happens |
| --- | --- |
| Server evaluation | Database executes the translated SQL |
| Client evaluation | C# application processes values in memory |

EF tries to translate the query. If an expression is unsupported in the final, top-level projection, EF can fetch its inputs and finish that projection in C#. An unsupported expression in a server-side filter generally causes a translation exception when executed. [Microsoft: client vs. server evaluation](https://learn.microsoft.com/en-us/ef/core/querying/client-eval)

#### The lecture's projection example

```csharp
// From my code: query_3 is commented out.
var query_3 =
    (from d in context.Departments
     select string.Join(':', "Dept", d.Name)).ToList();
```

In the lecture's version and overload, the mentor observes SQL selecting the name, followed by string formatting in application memory. A name such as `IT` becomes `Dept:IT`.

> **Version note:** Translation depends on the provider, EF version, and exact overload. Modern SQL Server providers translate some `string.Join` forms. Do not memorize “`string.Join` can never run on the server.” Check the mapping and generated SQL for the form being used. [Microsoft: SQL Server function mappings](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/functions)

#### A clearer example of the rule

```csharp
// Extra example: our own helper, with no configured SQL translation.
static string DisplayName(string name) => $"Dept:{name}";

var labels = context.Departments
    .Select(d => DisplayName(d.Name))
    .ToList();
```

The final projection can run the helper after fetching the required values. Putting the same helper in `Where` changes the problem:

```csharp
// Extra example: normally fails translation when executed.
var departments = context.Departments
    .Where(d => DisplayName(d.Name) == "Dept:IT")
    .ToList();
```

The database would need to evaluate an unknown method to decide which rows match. Moving `ToList` before `Where` would move filtering into application memory, but also fetch data before that filter. It is a different decision with a different cost, not a free fix.

### V2-7. Using EF.Functions

The two examples in `Program.cs` search for names containing `d`:

```csharp
// From my code.
var query_4 =
    from d in context.Departments
    where d.Name.Contains("d")
    select d;

var query_5 =
    from d in context.Departments
    where EF.Functions.Like(d.Name, "%d%")
    select d;
```

`Contains` expresses a C# string operation that the provider can translate. `EF.Functions.Like` explicitly requests SQL pattern matching.

| SQL pattern | Meaning |
| --- | --- |
| `%d%` | Contains `d` |
| `d%` | Starts with `d` |
| `%d` | Ends with `d` |

**Extra clarification:** `Contains` treats its input as substring text, while `Like` treats `%` and `_` as pattern characters. Their behavior is not interchangeable for every input. Case matching depends on database collation, and the exact generated SQL depends on the provider/version. [Microsoft: SQL Server function mappings](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/functions)

Both supplied queries are definitions only until executed. `EF.Functions.Like` is intended for a translated database query, not as a replacement for an ordinary in-memory string comparison.

### V2-8. Tracking and identity resolution

#### First: what is tracking for?

The `ChangeTracker` keeps information about entities associated with the context so that changes can be detected and saved. The lecture changes the default query behavior through the context constructor:

```csharp
// From the notes; this assignment is commented out in Context.cs.
public Context()
{
    ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
}
```

This changes the default for queries using that context. It does not make the objects immutable or prohibit later attaching them.

#### Why can the same department become multiple objects?

The lecture uses employees from the same department:

```csharp
// From my example: the AsNoTracking alternative.
var employees = context.Employees
    .AsNoTracking()
    .Include(e => e.Department)
    .ToList();
```

Suppose Employee 1 and Employee 2 both have `DepartmentID = 1`. The included department values describe the same database row. With plain no-tracking, repeated occurrences can become separate C# objects:

```text
Employee 1 -> Department object A, ID = 1
Employee 2 -> Department object B, ID = 1
```

They can have identical values without being the same reference.

The second option in the source is:

```csharp
var query_6 = context.Employees
    .AsNoTrackingWithIdentityResolution()
    .Include(e => e.Department);
```

Identity resolution reuses the object for repeated occurrences of an entity key within the query results:

```text
Employee 1 --+
            +--> One Department object, ID = 1
Employee 2 --+
```

#### Compare the choices

| Choice | Context tracks returned entities | Identity resolution |
| --- | --- | --- |
| Default tracking / `AsTracking()` | Yes | Through the context |
| `AsNoTracking()` | No | No |
| `AsNoTrackingWithIdentityResolution()` | No | Within that query's materialization |

The third option uses a temporary tracker for identity resolution, not the context's persistent tracker. Editing its results is not automatically saved by that context. [Microsoft: tracking and identity resolution](https://learn.microsoft.com/en-us/ef/core/querying/tracking)

**Correction:** The identity check is not a search through every object in application memory, and it does not imply an extra SQL lookup for each employee. It operates while EF creates the query's results. Reusing objects can reduce duplicates, but resolution itself has a cost; the total speed and memory benefit depends on the result shape. [Microsoft: identity resolution](https://learn.microsoft.com/en-us/ef/core/change-tracking/identity-resolution)

#### What if the default is NoTracking but I need to edit?

**Extra example:** Request tracking for that query:

```csharp
var employee = context.Employees
    .AsTracking()
    .First(e => e.ID == 1);
```

The lecture also mentions attaching a detached entity or changing its state. Those are separate editing steps. Do not equate “no tracking” with “this database row can never be updated.”

### V2-9. Global query filters

#### Start with soft delete

Hard delete removes the database row. Soft delete leaves it there and marks it, for example with `Deleted = true`.

From `Employee.cs`:

```csharp
public bool Deleted { get; set; }
```

Without a shared filter, each employee query needs to remember to exclude deleted employees. Forgetting the condition can produce a logical error even though the SQL runs successfully.

#### Configure the condition once

From `Context.cs`:

```csharp
modelBuilder.Entity<Employee>()
    .HasQueryFilter(e => !e.Deleted);
```

From `Program.cs`:

```csharp
var query_7 =
    from e in context.Employees
    where e.ID > 10
    select e;
```

Think of the effective condition as:

```text
Employee.ID > 10 AND Employee.Deleted = false
```

The second condition comes from the model configuration. It also applies when employees are queried through related-data loading. [Microsoft: global query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)

> **Remember:** “Global” applies to queries for the configured entity type. This code filters `Employee`, not every entity in the context. It does not turn `Remove` into soft delete; marking a row as deleted still requires an update of the flag.

**Extra example — intentionally including deleted employees:**

```csharp
var deletedEmployees = context.Employees
    .IgnoreQueryFilters()
    .Where(e => e.Deleted)
    .ToList();
```

If you simply add `Where(e => e.Deleted)` while the non-deleted global filter remains enabled, the two conditions conflict. Disabling filters is a deliberate exception to the default query behavior. [Microsoft: disabling query filters](https://learn.microsoft.com/en-us/ef/core/querying/filters#disabling-filters)

### V2-10. Shadow properties

#### Three different places to keep in mind

| Place | What it describes |
| --- | --- |
| C# entity class | Members directly accessible on an object |
| EF model | Properties and mappings known to EF |
| Database schema | Tables, columns, and constraints after schema changes are applied |

A shadow property belongs to the EF model without a corresponding property in the C# entity class. For this relational example, it is mapped to a database column when the schema is updated.

#### Configure it

From `Context.cs`:

```csharp
modelBuilder.Entity<Department>()
    .Property<bool>("Deleted")
    .IsRequired(true)
    .HasDefaultValue(false);
```

`Department` has no `Deleted` member, so EF defines a shadow property with that name. `bool` is its type, required means the mapped value is not nullable, and the database default is `false`.

**Question: should Deleted always be hidden from the class?** No. The mentor uses shadow properties to keep persistence metadata out of the class. It is a design choice, not a rule that `Deleted` is always a bad entity member. This same project intentionally keeps `Employee.Deleted` as a normal CLR property.

#### Access a tracked value

The source contains this commented example:

```csharp
var dept = context.Departments.First();
context.Entry(dept).Property("Deleted").CurrentValue = true;
context.SaveChanges();
```

The query returns a department under the current default tracking behavior. `Entry(...).Property(...)` accesses its tracked shadow value. `SaveChanges` saves that data change; it does not create the `Deleted` column.

#### Use it in a database query

From `Program.cs`:

```csharp
var query_8 =
    from d in context.Departments
    where EF.Property<bool>(d, "Deleted") == true
    select d;
```

Inside the query expression, `EF.Property` identifies the mapped property by name. It is not a normal runtime getter for arbitrary C# objects.

**Extra clarification:** A no-tracking result does not retain shadow values in the context's tracker for later access. You can still filter or project a shadow property in the SQL query before materialization. [Microsoft: shadow properties](https://learn.microsoft.com/en-us/ef/core/modeling/shadow-properties)

#### Extra example: combine a shadow property and a filter

```csharp
// Not currently configured for Department in the source.
modelBuilder.Entity<Department>()
    .HasQueryFilter(d => !EF.Property<bool>(d, "Deleted"));
```

Defining the shadow property and defining the filter are two separate actions. In the current source, `query_8` can ask for deleted departments because there is no global non-deleted filter on `Department`.

### V2-11. Applying properties to all entities

The lecture avoids repeating the same configuration for every entity. From `Context.cs`:

```csharp
foreach (var item in modelBuilder.Model.GetEntityTypes())
{
    modelBuilder.Entity(item.ClrType)
        .Property<bool>("Deleted")
        .IsRequired(true)
        .HasDefaultValue(false);

    modelBuilder.Entity(item.ClrType)
        .Property<DateTime>("CreatedDate")
        .IsRequired(true)
        .HasDefaultValueSql("GETDATE()");
}
```

The loop visits entity metadata currently present in this EF model. It is not a loop through every C# class in the project.

#### Question: why `item.ClrType` instead of `item.Name`?

`item.ClrType` is the actual C# `Type`, such as `typeof(Department)`. It selects the entity using the `Entity(Type)` overload, matching the ordinary CLR entities in this project.

`item.Name` is a metadata name string. It is not automatically the table name. The instructor demonstrates a string-based overload, but using a `Type` expresses directly which CLR entity this code is configuring. They are different API choices, not different spellings of the same value.

#### Question: what does `HasDefaultValueSql("GETDATE()")` do?

| Configuration | Database default |
| --- | --- |
| `HasDefaultValue(false)` | A constant value |
| `HasDefaultValueSql("GETDATE()")` | A SQL expression evaluated by SQL Server |

For an insert that uses this default, SQL Server supplies its current date/time. This is not the time `OnModelCreating` ran, and it does not automatically refresh on every update. Configuring the default must be followed by an appropriate schema update before it exists in the database. [Microsoft: generated properties and defaults](https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties)

#### What is actually shadow in this project?

| Entity | `Deleted` | `CreatedDate` |
| --- | --- | --- |
| `Employee` | Existing CLR property configured by the loop | Shadow property |
| `Department` | Shadow property | Shadow property |
| `Branch`, `Project`, `Attendance` | Shadow property | Shadow property |

If a property with that name already exists, `Property<bool>("Deleted")` configures it instead of creating a second property. The separate Department configuration before the loop repeats the same compatible settings. The loop adds property configuration, not filters for all entities. [Microsoft: shadow-property configuration](https://learn.microsoft.com/en-us/ef/core/modeling/shadow-properties)

**Question: why call `base.OnModelCreating(modelBuilder)`?** The mentor recalls inheritance: call the base implementation when extending its behavior. **Extra clarification:** The direct `DbContext` implementation does not add custom model rules of its own here; a custom base context may. Keep the project's call, but do not assume that this line creates its tables.

### V2-12. Recursive relationships and relationship fixup

The lecture ends with a research exercise: an employee manages employees, who may manage other employees. This relationship is not implemented in the supplied `Employee.cs`.

#### Extra example: describe one level of the relationship

```csharp
// Illustrative additions, not changes made to the source.
public int? ManagerID { get; set; }
public Employee? Manager { get; set; }
public ICollection<Employee> ManagedEmployees { get; set; }
    = new List<Employee>();
```

The relationship points back to the same entity type:

```text
Employee 1
  ├── Employee 2
  |     └── Employee 3
  └── Employee 4
```

An employee has one manager reference and potentially many managed employees. A nullable manager key allows a top-level employee with no manager.

**Extra example — explicit mapping:**

```csharp
modelBuilder.Entity<Employee>()
    .HasOne(e => e.Manager)
    .WithMany(e => e.ManagedEmployees)
    .HasForeignKey(e => e.ManagerID)
    .OnDelete(DeleteBehavior.Restrict);
```

`Restrict` is an illustrative choice to avoid cascading deletion down this management relationship; it is not a configuration from the video.

#### What relationship fixup does

When related entities are available to the context, EF aligns their navigation references with their FK values. If Employee 2 has `ManagerID = 1` and both employees are loaded and tracked, EF can connect Employee 2 to Employee 1 and populate the corresponding collection. [Microsoft: relationship fixup](https://learn.microsoft.com/en-us/ef/core/change-tracking/relationship-changes)

**Correction:** Fixup connects available objects. It does not send queries to discover every descendant. Repeated `ThenInclude` follows a specified depth; it is not unlimited recursion.

**Extra clarification — conceptual approach:** Loading a suitable set of employees in one tracking query can let fixup connect the loaded manager/employee references. It builds only the graph represented by those results. Employees excluded by a filter or absent from the result do not appear by magic.

| Query behavior | Connection to this exercise |
| --- | --- |
| Tracking | Context can reuse and connect loaded entities across queries |
| Plain no-tracking | No context-level fixup across independently loaded results |
| No-tracking with identity resolution | Reuses identities within one result; does not load missing descendants or keep results tracked |

The last option supports identity consistency, not automatic recursive loading. This is why the mentor asks to study fixup together with the tracking choices. [Microsoft: identity resolution](https://learn.microsoft.com/en-us/ef/core/change-tracking/identity-resolution)

### V2-13. What is currently active in my project?

The code preserves the successive lecture experiments. It is not one complete demonstration that executes all of them together.

| Example or setting | Current source state |
| --- | --- |
| Initial query expression | Commented out |
| Eager-loading query with `AsSingleQuery` | Commented out |
| `query_2 = context.Departments.ToList()` | Executes if earlier context/schema work succeeds |
| Explicit-loading loop | Commented out |
| `query_3`, client/server example | Commented out |
| `query_4` and `query_5`, string queries | Defined but not executed |
| `query_6`, identity-resolution query | Defined but not executed |
| `query_7`, employee query | Defined but not executed |
| `query_8`, department shadow-property query | Defined but not executed |
| Department shadow-value update and `SaveChanges` | Commented out |
| Logging | Commented out |
| Lazy-loading proxy configuration | Commented out; package and virtual navigations present |
| Constructor's default NoTracking setting | Commented out; ordinary entity queries keep default tracking |
| Employee global query filter | Configured |
| Deleted / CreatedDate property configuration | Configured in the EF model |
| Recursive manager relationship | Research exercise; absent from the source |

The supplied migration files still contain the four Video 1 migrations. They do not contain the new `Deleted` and `CreatedDate` configuration. Therefore, the source model and checked-in migration baseline do not yet match for those features. This describes the files; it does not assert what columns exist in the local database.

For a future experiment, model/schema changes such as new columns need a reviewed migration and application to the database. Loading choices, query filters, and tracking options do not by themselves create columns. No database changes were performed while writing these notes.

### V2-14. Revision questions and decisions

#### Explain the flow before memorizing the method

```text
Describe the query and requested result
              |
              v
Execute: ToList / First / enumeration
              |
              v
Provider translates the supported work into SQL
              |
              v
Database returns matching rows
              |
              v
EF creates the result objects and applies the chosen
tracking / identity-resolution behavior
```

| Question | Short answer |
| --- | --- |
| Does `ToList` fix multiple-collection joins? | No; it executes the chosen query strategy |
| Does `AsSplitQuery` mean no joins anywhere? | No; it splits collection loading |
| Are split queries always faster? | No; weigh repeated data against round trips |
| Does `Query().Where(...)` execute immediately? | No; execute or enumerate the query |
| Can a filtered query remove objects already loaded into a navigation? | No; use its own filtered result |
| Does projection need full entities? | No; select the values required |
| Does `virtual` enable lazy loading on its own? | No; the loading mechanism must be configured |
| Does no-tracking mean an immutable object? | No; it means no automatic context tracking of that result |
| Is identity resolution shared across all application memory? | No; its scope depends on the tracking choice |
| Does a query filter delete rows? | No; it changes which rows ordinary queries return |
| Does adding Deleted add a filter automatically? | No; configure the filter separately |
| Is Employee.Deleted shadow in this project? | No; it already exists in the C# class |
| Does `SaveChanges` create a shadow property's column? | No; schema changes are handled separately |
| Does GETDATE run whenever I read the entity? | No; it is the configured SQL default for insertion |
| Does relationship fixup load missing employees? | No; it connects available entities |

#### Choose based on the operation

| Need | Starting choice to consider |
| --- | --- |
| Related entities are definitely required | Eager loading |
| One navigation is needed only at a later decision point | Explicit loading |
| A display needs selected values | Projection |
| Navigation access should fetch data on demand | Lazy loading, after assessing hidden query costs |
| Results will be edited and saved through this context | Tracking |
| Read-only use with little repeated entity data | Plain no-tracking |
| Read-only graph with repeated entity identities | No-tracking with identity resolution; assess its cost |
| Exclude soft-deleted rows consistently | Global query filter |
| Map metadata without a CLR property | Shadow property |

These are starting points, not rules that one method always wins. Follow the mentor's central habit: explain what the operation needs and what EF will do before choosing the API.
