# Random Utils

Zero-dependency randomization helpers in the `Sroglu.Toolbox.Randomization`
namespace. Pure C# — no engine references. `RandomUtils` is a static class whose
methods each accept an optional `System.Random`; omit it to use a shared instance,
or pass your own for deterministic, seedable results.

- **`Pick`** — uniform choice from a list.
- **`PickWeighted`** — choice proportional to per-element weights.
- **`Shuffle`** — in-place Fisher-Yates.
- **`Range`** — `int` or `float` in a half-open interval.
- **`Chance`** — a probability-`0..1` coin flip.

## Import

Package Manager → **+ → Add package from git URL…**:

```
https://github.com/sroglu/Toolbox.git?path=/Packages/com.sroglu.toolbox.random#main
```

## Usage

```csharp
using Sroglu.Toolbox.Randomization;

string loot = RandomUtils.Pick(items);
string rare = RandomUtils.PickWeighted(items, weights);

RandomUtils.Shuffle(deck);

int roll = RandomUtils.Range(1, 7);        // 1..6
float jitter = RandomUtils.Range(-0.5f, 0.5f);

if (RandomUtils.Chance(0.25f))
    DropBonus();

// Deterministic: pass your own seeded generator.
var rng = new System.Random(12345);
RandomUtils.Shuffle(deck, rng);
```
