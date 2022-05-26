# NGen
A procedural text generator orientated towards generating short texts - like names.

Text generators are written in simple text files. At their simplest they consist of a named generator, for example:

`hi = Hello World`

The minimum that is necessary for a generator is a name, followed by an `=` sign, followed by some text which will be returned when that generator is run. In the above example, every time the hi generator is run it will return the same text: "Hello World".

---

The following are the currently available features:

## Word Lists

Word Lists are the basic unit of construction for most name generators. They are contained within square brackets `[]` and elements within them are separated by commas `,`:

`name = [Rupert, Marjory, Caleb, Alba]`

This will output a single name from the list each time.

### Nested Lists

Lists can be nested, eg:

`name = [[Jason, Alberto, Vince], [Wendy, Samantha, Paula]]`

This will still output a single name each time, from one or other of the nested lists

### Text outside lists

Not all text need be inside a list. Text outside of lists will always be reproduced, eg:

`name = Mr [Thronton, Cleeson, Speg]`

This will output a single random name each time, but always starting with "Mr"

## Referencing generators
Writing complicated generators in one go can be difficult, so instead the task can be split into multiple generators.
One generator can reference the output of another generator by using its name preceeded by a `$` symbol, like so:

```
honorific = [Mr, Ms, Mrs, Miss, Master, Sir, Madam]
surname = [Clancey, Dumblethorn, Addlington, Cranch, Asperly]

name = $honorific $surname
```

When the "name" generator is run, it will access the "honorific" and "surname" generators to provide an output (the "honorific" and "surname" generators can also still be run separately).
These generator references can be included in lists and nested.

## Comments

Coments can be written along with generators, by starting lines with a `#` symbol:

```
# This is a comment and will be ignored

gen = this is not a comment and will create a generator
```
Every commented line must have a `#` as its first character.

## Multiline generators

Complicated generators take space to construct and doing so on a single line can be tricky and hard to read, so longer definitions can be written on multiple lines

```
multilineVeg = [
	artichoke, beetroot, carrot, cauliflower, celery, 
	endive, kale, leek, marrow, pea, 
	potato, pumpkin, spinach, squash, yarrow
]
```

In general, whitespace such as tabs, linebreaks and spaces, are ignored. There are some exceptions to this, such as every commented line having to have a `#` character at the beginning of it.

## Escaping Characters

As we have seen, NGen uses special characters like `[` and `$` to define things like Lists and References, the complete list of these special characters is:

`# = [ ] , $`

Sometimes you want to use these characters in your text, to do so they need to be escaped by putting a `\` character before them, otherwise they will be read as part of your generator structure, and not part of its content. eg:

```
#these dollar-signs will be read as references to generators named '100' and '200',
#generators which probably don't exist
money = [$100, $200]

#the dollar signs should be escaped like this:
money2 = [\$100, \$200]

```