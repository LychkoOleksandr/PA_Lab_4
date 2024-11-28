namespace Lab_4_PA;

internal abstract class Program
{
    private static readonly Random Random = new();

    private const int Capacity = 250;
    private const int NumItems = 100;
    private const int PopulationSize = 100;
    private const int Generations = 100;
    private const double CrossoverRate = 0.3;
    private const double MutationRate = 0.1;

    private static void Main()
    {
        var items = GenerateItems();
        var population = InitializePopulation();

        for (int generation = 0; generation < Generations; generation++)
        {
            var offspring = PerformCrossover(population);
            Mutate(offspring);
            ApplyLocalImprovement(offspring, items);

            population = SelectNextGeneration(population, offspring, items);
            if ((generation + 1) % 10 == 0)
            {
                Console.WriteLine($"Generation {generation + 1}: Best fitness = {EvaluatePopulation(population, items).Max()}");
            }
        }
    }

    private static List<Item> GenerateItems()
    {
        var items = new List<Item>();
        for (int i = 0; i < NumItems; i++)
        {
            items.Add(new Item
            {
                Value = Random.Next(2, 31),
                Weight = Random.Next(1, 26)
            });
        }
        return items;
    }

    private static List<bool[]> InitializePopulation()
    {
        var population = new List<bool[]>();
        for (int i = 0; i < PopulationSize; i++)
        {
            var chromosome = new bool[NumItems];
            chromosome[Random.Next(NumItems)] = true;
            population.Add(chromosome);
        }
        return population;
    }

    private static List<bool[]> PerformCrossover(List<bool[]> population)
    {
        var offspring = new List<bool[]>();
        for (int i = 0; i < population.Count; i += 2)
        {
            if (i + 1 < population.Count && Random.NextDouble() < CrossoverRate)
            {
                var parent1 = population[i];
                var parent2 = population[i + 1];
                var (child1, child2) = TwoPointCrossover(parent1, parent2);
                offspring.Add(child1);
                offspring.Add(child2);
            }
            else
            {
                offspring.Add(population[i]);
                if (i + 1 < population.Count) offspring.Add(population[i + 1]);
            }
        }
        return offspring;
    }

    private static (bool[], bool[]) TwoPointCrossover(bool[] parent1, bool[] parent2)
    {
        int point1 = Random.Next(NumItems);
        int point2 = Random.Next(NumItems);
        if (point1 > point2) (point1, point2) = (point2, point1);

        var child1 = (bool[])parent1.Clone();
        var child2 = (bool[])parent2.Clone();

        for (int i = point1; i <= point2; i++)
        {
            child1[i] = parent2[i];
            child2[i] = parent1[i];
        }

        return (child1, child2);
    }

    private static void Mutate(List<bool[]> population)
    {
        foreach (var chromosome in population)
        {
            if (!(Random.NextDouble() < MutationRate)) continue;
            int gene1 = Random.Next(NumItems);
            int gene2 = Random.Next(NumItems);

            (chromosome[gene1], chromosome[gene2]) = (chromosome[gene2], chromosome[gene1]);
        }
    }

    private static void ApplyLocalImprovement(List<bool[]> population, List<Item> items)
    {
        foreach (var chromosome in population)
        {
            for (int i = 0; i < NumItems; i++)
            {
                if (chromosome[i]) continue;
                chromosome[i] = true;
                if (GetWeight(chromosome, items) > Capacity)
                {
                    chromosome[i] = false;
                }
            }
        }
    }

    private static List<bool[]> SelectNextGeneration(List<bool[]> population, List<bool[]> offspring, List<Item> items)
    {
        List<bool[]> combined = population.Concat(offspring).ToList();
        List<int> fitnesses = EvaluatePopulation(combined, items);
        return combined
            .Select((chromosome, index) => (chromosome, fitness: fitnesses[index]))
            .OrderByDescending(x => x.fitness)
            .Select(x => x.chromosome)
            .Take(PopulationSize)
            .ToList();
    }

    private static List<int> EvaluatePopulation(List<bool[]> population, List<Item> items)
    {
        return population.Select(chromosome => GetFitness(chromosome, items)).ToList();
    }

    private static int GetFitness(bool[] chromosome, List<Item> items)
    {
        int value = 0, weight = 0;
        for (int i = 0; i < NumItems; i++)
        {
            if (!chromosome[i]) continue;
            value += items[i].Value;
            weight += items[i].Weight;
        }
        return weight <= Capacity ? value : 0;
    }

    private static int GetWeight(bool[] chromosome, List<Item> items)
    {
        int weight = 0;
        for (int i = 0; i < NumItems; i++)
        {
            if (chromosome[i]) weight += items[i].Weight;
        }
        return weight;
    }
}

internal class Item
{
    public int Value { get; init; }
    public int Weight { get; init; }
}
