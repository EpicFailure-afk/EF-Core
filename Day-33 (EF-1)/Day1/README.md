# Day-2

## Navigation Props

 - group of properties that link classes to each other
 - auto generated for Example --> `Day-33 (EF-1)\Day1\Department.cs`
   - at **Department.cs**:
   <br>
 
   ```cs
   // an object from Employee
   public virtual Employee Employee { get; set; }
   [
     System.Diagnostics.CodeAnalysis
     .SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")
    ]

   // Collection of Employees
   public virtual ICollection<Employee> Employees { get; set; }
   
   ```
 
<hr>

## Lazy loading 
 - when object load from database the navigation props did not load with it 
   unless we start querying on them
 - while declaring the navigation prop must declare it as *virtual* 
   so we can perform **lazy loading** 
- if there isn't *virtual* will get the object and all its data means there will not be lazy loading 

<br>

### diff between deferred execution and Lazy Loading 
in case of **Department.cs** which contains an object/collection (*virtual*) from Employee
- either data from **Department** came as deferred or eager
  - data of **Employee** will not come with it unless we query on **Employee** to fetch data from

<hr>

## Functions:

```cs
  var dept_1 = context.Departments.Find(3);
```

- **Find(3)**: return the record or object of id 3


#### diff between **Find()** and **Single()**
**Find():** firstly it searches in memory, if that object already exits in memory
  'Find' will retrieve that object from the memory instead going to database

**Single:** if the object is not exist at memory, 'Single' will fetches it from database
but this object must be single, if the data retrieved are multiple data that will throw an exception  


<hr>

## change tracker **context**

```cs
var dept_2 = context.Departments.First();
dept_2.Name = "Intake 42";
context.SaveChanges();
```

<br>

- responsible to track changes on object level which **context** will fetch from database
- **context**: is the layer between the client and the server (database)
  we use **context** to query on database
  - default action --> any object come through **context** will be tracked 
    - **context** carries a *ref* to that object 
      - that *ref* saved into class **Entry** 
      - each record fetched from database will save as object from the class **Entry**
      - and that means **==leak at performance==**
      - the class **Entry** has prop named **state**: status of that object
        - this state is Enum has some values one of them is modify
        - `context.SaveChanges();` --> detect changes: iterate on all entries that with context 
          to know which **state** is modified 
        


<hr>

## know it's exist

```cs
context.Entry(dept_2).State = System.Data.Entity.EntityState.Added;
```
<br>

> TO MODIFY THE STATE MANUALLY ---> know it exists 
 - Added --> new object that is not stored at context (insert)
 - Deleted --> deleted from context but still exists at database
 - Modified --> changed (update)
 - Unchanged --> not changed 
 - Detached --> make this object untracked, do not track this Entry



<hr>


## read only data

```cs
      var query_4 =
        (from d in context.Departments
         where d.ID > 10
         select d.ID).AsNoTracking();
```

> .AsNoTracking()
- context will not track them
- means any change will affect the data in memory only