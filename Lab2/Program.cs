// Tema_CSharp9_Complexa.pdf


using System;
using System.Collections.Generic;
using System.Linq;

DemoRecordAndWith();
DemoInitOnlyProperties();
DemoTopLevelInteraction();

var studentExample = new Student(2, "Ion Pop", 21, new List<Course> { new("Fizică", 4) });
var courseExample = new Course("Chimie", 3);
DemoPatternMatchingv1(studentExample);
DemoPatternMatchingv2(courseExample);
DemoPatternMatchingv2(123);

DemoLambdaFilter();
void DemoRecordAndWith()
{
    var math = new Course("Matematica", 5);
    var cs = new Course("Programare C#", 4);
    
    var student1 = new Student(1, "Ana Popescu", 20, new List<Course> { math});
    var student2 = student1 with {Courses = new List<Course>(student1.Courses){ cs }};
    
    Console.WriteLine("=== Record & With ===");
    Console.WriteLine($"Student initial: {student1.Name}, cursuri: {string.Join(", ", student1.Courses.Select(c => c.Title))}");
    Console.WriteLine($"Student clonat:  {student2.Name}, cursuri: {string.Join(", ", student2.Courses.Select(c => c.Title))}");
    Console.WriteLine();
}
void DemoInitOnlyProperties()
{
    var instructor = new Instructor
    {
        Name = "Olariu Florin",
        Department = "Informatica",
        Email = "olariuflorin@uaic.ro"
    };

    Console.WriteLine("=== Init-only Properties ===");
    Console.WriteLine(instructor);
    Console.WriteLine();
}

void DemoTopLevelInteraction()
{
    Console.WriteLine("=== Adaugare studenti ===");
    var students = new List<string>();
    while (true)
    {
        Console.Write("Introdu numele studentului (sau ENTER pt a te opri): ");
        var nume =  Console.ReadLine();
        if (string.IsNullOrEmpty(nume))
            break;
        students.Add(nume);
    }
    Console.WriteLine("\nLista completă de studenti:");
    foreach (var s in students)
        Console.WriteLine($"- {s}");
    Console.WriteLine();
}

void DemoPatternMatchingv1(object obj)
{
    Console.WriteLine("=== Pattern Matching with ifs ===");
    
    if (obj is Student s && s.Courses.Count > 0)
    {
        Console.WriteLine($"Student: {s.Name}, numar cursuri: {s.Courses.Count}");
    }else if (obj is Student s2)
    {
        Console.WriteLine($"Student: {s2.Name}, fara cursuri");
    }else if (obj is Course c)
    {
        Console.WriteLine($"Course: {c.Title}, credite: {c.Credits}");
    }else{
        Console.WriteLine("Tip necunoscut");
    }
    Console.WriteLine();
}

void DemoPatternMatchingv2(object obj)
{
    Console.WriteLine("=== Pattern Matching with switch===");

    switch (obj)
    {
        case Student s when s.Courses.Count > 0:
            Console.WriteLine($"Student: {s.Name}, numar cursuri: {s.Courses.Count}");
            break;
        case Student s:
            Console.WriteLine($"Student: {s.Name}, fara cursuri");
            break;
        case Course c:
            Console.WriteLine($"Curs: {c.Title}, credite: {c.Credits}");
            break;
        default:
            Console.WriteLine("Tip necunoscut");
            break;
    }

    Console.WriteLine();
}

void DemoLambdaFilter()
{
    Console.WriteLine("=== Demo Lambda Filter ===");
    var courses = new List<Course>
    {
        new Course("Programare C#", 4),
        new Course("Baze de date", 3),
        new Course("IP", 5),
        new Course("Engleza", 2)
    };
    Func<Course, bool> filter = static c => c.Credits > 3;
    var filteredCourses = courses.Where(filter);
    Console.WriteLine("Cursuri cu mai mult de 3 credite:");
    foreach (var c in filteredCourses)
    {
        Console.WriteLine($"- {c.Title} ({c.Credits} credite)");
    }
    Console.WriteLine();
}
public record Course(string Title, int Credits);
public record Student(int Id, string Name, int Age, List<Course> Courses);

public class Instructor
{
    public string Name { get; init; }
    public string Department { get; init; } 
    public string Email { get; init; }

    public override string ToString() => $"{Name} ({Department}) - {Email}";
}