namespace CoAuthors
{
    public class CoAuthors
    {
        public Person PersonA { get; set; }
        public Person PersonB { get; set; }
        public long Count { get; set; }

        public CoAuthors(Person personA, Person personB)
        {
            PersonA = personA;
            PersonB = personB;
            Count = 1;

            PersonA.Coauthors.Add(this);
            PersonB.Coauthors.Add(this);
        }

        public static List<CoAuthors> CAs = new List<CoAuthors>();
        public static void Create(Person personA, Person personB)
        {
            foreach(var ca in personA.Coauthors)
            {
                if(ca.PersonA == personA || ca.PersonA == personB)
                {
                    ca.Count += 1;
                    return;
                }
            }

            CAs.Add(new CoAuthors(personA, personB));
        }
    }
}
