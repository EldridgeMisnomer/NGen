# NGen
A procedural text generator orientated towards generating short texts - like names.

Text generators are written in simple text files. At their simplest they consist of a named generator, for example:

`hi = Hello World`

The minimum that is necessary for a generator is a name, followed by an `=` sign, followed by some text which will be returned when that generator is run. In the above example, every time the hi generator is run it will return the same text: "Hello World".

At the moment, generators must be written on a single line. This will change at some point.

---

The following are the currently available features:

### Randomly accessed lists of words

Lists are the basic unit of construction for most name generators. They are contained within square brackets `[]` and elements within them are separated by commas `,`:

`name = [Rupert, Marjory, Caleb, Alba]`

This will output a single name from the list each time.

### Nested lists

Lists can be nested, eg:

`name = [[Jason, Alberto, Vince], [Wendy, Samantha, Paula]]`

This will still output a single name each time, from one or other of the nested lists

### Text outside lists

Not all text need be inside a list. Text outside of lists will always be reproduced, eg:

`name = Mr [Thronton, Cleeson, Speg]`

This will output a single random name each time, but always starting with "Mr"

### Referencing generators
Writing complicated generators in one go can be difficult, so instead the task can be split into multiple generators.
One generator can reference the output of another generator by using its name preceeded by a `$` symbol, like so:

```
honorific = [Mr, Ms, Mrs, Miss, Master, Sir, Madam]
surname = [Clancey, Dumblethorn, Addlington, Cranch, Asperly]

name = $honorific $surname
```

when the "name" generator is run, it will access the "honorific" and "surname" generators to provide an output (the "honorific" and "surname" generators could also still be run separately).
These generator references can be included in lists and nested.
