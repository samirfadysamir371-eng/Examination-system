

enum Level { Easy = 1, Medium = 2, Hard = 3 }

abstract class Question
{
    public string Header { get; set; }
    public int Marks { get; set; }
    public Level Level { get; set; }

    protected Question(string header, int marks, Level level)
    {
        Header = header;
        Marks = marks;
        Level = level;
    }

    public virtual void Display()
    {
        Console.WriteLine($"\n[{Level}] {Header} ({Marks} marks)");
    }

    public abstract bool CheckAnswer(string answer);
}

class TrueFalseQuestion : Question
{
    public bool CorrectAnswer { get; set; }

    public TrueFalseQuestion(string header, int marks, Level level, bool correct)
        : base(header, marks, level)
    {
        CorrectAnswer = correct;
    }

    public override void Display()
    {
        base.Display();
        Console.WriteLine("Answer (true / false):");
    }

    public override bool CheckAnswer(string answer)
    {
        string a = answer.Trim().ToLower();
        if (a == "true" || a == "t") return CorrectAnswer == true;
        if (a == "false" || a == "f") return CorrectAnswer == false;
        return false;
    }
}

class ChooseOneQuestion : Question
{
    public string[] Choices { get; set; }
    public int CorrectChoice { get; set; }

    public ChooseOneQuestion(string header, int marks, Level level, string[] choices, int correct)
        : base(header, marks, level)
    {
        Choices = choices;
        CorrectChoice = correct;
    }

    public override void Display()
    {
        base.Display();
        for (int i = 0; i < Choices.Length; i++)
            Console.WriteLine($"{i + 1}. {Choices[i]}");
        Console.WriteLine("Enter choice number:");
    }

    public override bool CheckAnswer(string answer)
    {
        return int.TryParse(answer.Trim(), out int n) && n == CorrectChoice;
    }
}

class MultipleChoiceQuestion : Question
{
    public string[] Choices { get; set; }
    public HashSet<int> CorrectChoices { get; set; }

    public MultipleChoiceQuestion(string header, int marks, Level level, string[] choices, HashSet<int> correct)
        : base(header, marks, level)
    {
        Choices = choices;
        CorrectChoices = correct;
    }

    public override void Display()
    {
        base.Display();
        for (int i = 0; i < Choices.Length; i++)
            Console.WriteLine($"{i + 1}. {Choices[i]}");
        Console.WriteLine("Enter correct answers (comma separated, e.g. 1,3):");
    }

    public override bool CheckAnswer(string answer)
    {
        var given = new HashSet<int>();
        foreach (string part in answer.Split(','))
        {
            if (!int.TryParse(part.Trim(), out int n)) return false;
            given.Add(n);
        }
        return given.SetEquals(CorrectChoices);
    }
}

class Program
{
    static List<Question> questionBank = new List<Question>();

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n===== Examination System =====");
            Console.WriteLine("1. Doctor Mode");
            Console.WriteLine("2. Student Mode");
            Console.WriteLine("3. Exit");
            int choice = ReadInt("Choose: ", 1, 3);

            if (choice == 1) DoctorMode();
            else if (choice == 2) StudentMode();
            else break;
        }
    }

    static void DoctorMode()
    {
        Console.WriteLine("\n--- Doctor Mode ---");
        int count = ReadInt("How many questions to add? ", 1, 100);

        for (int i = 1; i <= count; i++)
        {
            Console.WriteLine($"\nQuestion {i}:");
            Console.WriteLine("1. True/False  2. Choose One  3. Multiple Choice");
            int type = ReadInt("Select type: ", 1, 3);
            Console.WriteLine("1. Easy  2. Medium  3. Hard");
            Level level = (Level)ReadInt("Enter level: ", 1, 3);

            Console.Write("Enter question header: ");
            string header = Console.ReadLine();
            int marks = ReadInt("Enter marks: ", 1, 100);

            if (type == 1)
            {
                bool correct = ReadBool("Correct answer (true/false): ");
                questionBank.Add(new TrueFalseQuestion(header, marks, level, correct));
            }
            else
            {
                string[] choices = new string[4];
                for (int c = 0; c < 4; c++)
                {
                    Console.Write($"Choice {c + 1}: ");
                    choices[c] = Console.ReadLine();
                }

                if (type == 2)
                {
                    int correct = ReadInt("Correct choice number (1-4): ", 1, 4);
                    questionBank.Add(new ChooseOneQuestion(header, marks, level, choices, correct));
                }
                else
                {
                    HashSet<int> correct = ReadCorrectSet();
                    questionBank.Add(new MultipleChoiceQuestion(header, marks, level, choices, correct));
                }
            }
            Console.WriteLine("Question added.");
        }
    }

    static void StudentMode()
    {
        Console.WriteLine("\n--- Student Mode ---");
        Console.WriteLine("1. Practical  2. Final");
        int examType = ReadInt("Choose exam type: ", 1, 2);
        Console.WriteLine("1. Easy  2. Medium  3. Hard");
        Level level = (Level)ReadInt("Choose level: ", 1, 3);

        List<Question> pool = questionBank.Where(q => q.Level == level).ToList();
        if (pool.Count == 0)
        {
            Console.WriteLine("No questions available for this level.");
            return;
        }

        List<Question> exam = examType == 1
            ? pool.Take(Math.Max(1, pool.Count / 2)).ToList()
            : pool;

        int score = 0, total = 0;
        foreach (Question q in exam)
        {
            q.Display();
            string answer = Console.ReadLine();
            total += q.Marks;
            if (q.CheckAnswer(answer)) score += q.Marks;
        }

        Console.WriteLine($"\nYour Result: {score} / {total}");
    }

    static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int n) && n >= min && n <= max)
                return n;
            Console.WriteLine($"Invalid input. Enter a number between {min} and {max}.");
        }
    }

    static bool ReadBool(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string a = Console.ReadLine().Trim().ToLower();
            if (a == "true" || a == "t") return true;
            if (a == "false" || a == "f") return false;
            Console.WriteLine("Invalid input. Enter true or false.");
        }
    }

    static HashSet<int> ReadCorrectSet()
    {
        while (true)
        {
            Console.Write("Correct answers (comma separated, e.g. 1,3): ");
            var set = new HashSet<int>();
            bool ok = true;
            foreach (string part in Console.ReadLine().Split(','))
            {
                if (int.TryParse(part.Trim(), out int n) && n >= 1 && n <= 4) set.Add(n);
                else { ok = false; break; }
            }
            if (ok && set.Count > 0) return set;
            Console.WriteLine("Invalid input. Use numbers 1-4 separated by commas.");
        }
    }
}