// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Reflection.Metadata.Ecma335;

public class Mediator
{
    static void Main(string[] args)
    {
        var room = new ChatRoom();

        var john = new Person("John");
        var jane = new Person("Jane");

        room.Join(john);
        room.Join(jane);

        john.Say("hi room");
        jane.Say("oh, hey john");

        var simon = new Person("Simon");
        room.Join(simon);
        simon.Say("hi everyone!");

        jane.PrivateMessage(simon.Name, "glad you could join us!");
    }


}

public class Person
{
    public string Name { get; set; }
    public ChatRoom Room { get; set; }
    private List<string> chatlogs = new List<string>();

    public Person(string name)
    {
        Name = name;
    }

    public void Say(string message)
    {
        Room.BroadCast(Name, message);
    }
    public void PrivateMessage(string who, string message)
    {
        Room.Message(Name, who, message);
    }
    public void Receive(string sender, string message)
    {
        string s = $"{sender}: '{message}'";
        chatlogs.Add(s);
        Console.WriteLine($"[{Name}'s Chat Session] {s}");
    }
}

public class ChatRoom
{
    private List<Person> People = new List<Person>();
    public void Join(Person p)
    {
        string joinMsg = $"{p.Name} has joined the chat";
        BroadCast("room", joinMsg);

        p.Room = this;
        People.Add(p);
    }

    public void BroadCast(string source, string message)
    {
        foreach (var person in People)
        {
            if (person.Name != source)
            {
                person.Receive(source, message);
            }
        }
    }
    public void Message(string source, string destination, string message)
    {
        People.FirstOrDefault(f => f.Name == destination)?.Receive(source, message);
    }
}