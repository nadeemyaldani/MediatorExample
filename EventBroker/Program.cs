using Autofac;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static System.Console;

namespace TestEventBroker
{
    public class ProgramEventBroker
    {
        static void Main(string[] args)
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<EventBroker>().SingleInstance();
            cb.RegisterType<FootballCoach>();
            
            cb.Register((c, p) =>
            new FootballPlayer(
                c.Resolve<EventBroker>(),
                p.Named<string>("name")
                ));

            using (var c = cb.Build())
            {
                var coach = c.Resolve<FootballCoach>();
                var Player1 = c.Resolve<FootballPlayer>(new NamedParameter("name", "Jhon"));
                var Player2 = c.Resolve<FootballPlayer>(new NamedParameter("name", "Chris"));


                Player1.Score();
                Player1.Score();
                Player1.Score(); // should be ignored
                Player1.AssualtReferee();
                Player2.Score();
            }
        }
    }

    public class Actor
    {
        protected EventBroker broker;
        public Actor(EventBroker broker)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));

        }
    }

    public class FootballPlayer : Actor
    {
        public string Name { get; set; }
        public FootballPlayer(EventBroker broker, string name) : base(broker)
        {
            Name = name ?? throw new ArgumentNullException(paramName: nameof(name));

            broker.OfType<PlayerScoreEvent>()
               .Where(w => !w.Name.Equals(Name))
               .Subscribe(pe =>
                {
                    WriteLine($"{name}: Nicely done {pe.Name}, it's your {pe.GoalsScored}!");
                });

            broker.OfType<PlayerSentOffEvent>()
               .Where(w => w.Name.Equals(Name))
               .Subscribe(pe =>
               {
                   WriteLine($"{name}: won't happen again.");
               });

            broker.OfType<PlayerSentOffEvent>()
               .Where(w => !w.Name.Equals(Name))
               .Subscribe(pe =>
               {
                   WriteLine($"{name}: see you in the lookers, {pe.Name}");
               });


        }
        public int GoalsScored { get; set; }

        public void Score()
        {
            GoalsScored++;
            broker.Publish(new PlayerScoreEvent { Name = Name, GoalsScored = GoalsScored });
        }

        public void AssualtReferee()
        {
            broker.Publish(new PlayerSentOffEvent { Name = Name, Reason = "violence" });
        }
    }
    public class FootballCoach : Actor
    {
        public FootballCoach(EventBroker broker) : base(broker)
        {

            broker.OfType<PlayerScoreEvent>()
                .Subscribe(pe =>
                {
                    if (pe.GoalsScored < 3)
                        WriteLine($"Coach: well done, {pe.Name}!");
                });
            broker.OfType<PlayerSentOffEvent>()
                .Subscribe(pe =>
                {
                    if (pe.Reason == "violence")
                        WriteLine($"Coach: how could you, {pe.Name}!");
                }
                );
        }
    }

    public class PlayerEvent
    {
        public string Name { get; set; }
    }
    public class PlayerScoreEvent : PlayerEvent
    {
        public int GoalsScored { get; set; }

    }
    public class PlayerSentOffEvent : PlayerEvent
    {
        public string Reason { get; set; }
    }
    public class EventBroker : IObservable<PlayerEvent>
    {
        Subject<PlayerEvent> subsriptions = new Subject<PlayerEvent>();
        public IDisposable Subscribe(IObserver<PlayerEvent> observer)
        {
            return subsriptions.Subscribe(observer);
        }
        public void Publish(PlayerEvent pe)
        {
            subsriptions.OnNext(pe);
        }
    }
}