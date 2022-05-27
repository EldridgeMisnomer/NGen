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

### Pick Types

The word which is selected for output from a list can be picked in one of three basic ways: random, shuffle, and cycle.
At the moment, the Pick Type can only be set in the Header (see [Setting Pick Type in the Header](#setting-pick-type-in-the-header)) for more information), this will change soon.

#### Random pick type

The default Pick Type is random - one of the elements from the list will be selected at random each time.

```
^
	pick = random
^

randomgen = [harpsichord, lute, harmonium, flute, harp, oboe]
```
Each time the generator is run, any one of the elements in the list has an equal chance of being picked

#### Shuffle pick type

When the Pick Type is set to shuffle, elements are still picked randomly, but instead of picking a random one each time, the whole list is reordered randomly before the generator is ever run, and then the elements are picked in order, one at a time. When all the elements have been picked, the list is reordered randomly again and picking starts again at the beginning.
This mostly avoids the same element being picked twice in a row (it can still happen directly after a shuffle), and ensures that all elements from the list are seen given enough picks.

```
^
	pick = shuffle
^

shufflegen = [bee, spider, caterpillar, beetle]
```
In the above example, the list might be reordered to `[spider, beetle, caterpillar, bee]` and will then output first 'spider', then 'beetle', then 'caterpillar'. Once 'bee' has been picked the list is shuffled again.

#### Cycle pick type

When the Pick Type is set to cycle, elements are not picked randomly at all, but instead in the order they were written in. Once the last element is reached the first will be picked next.

```
^
	pick = cycle
^

cyclegen = [shrub, bush, tree] 
```
The above will always output elements in the same order: 'shrub', then 'bush', then 'tree' then 'shrub' and so on.

## Text outside lists

Not all text need be inside a list. Text outside of lists will always be reproduced, eg:

`name = Mr [Thronton, Cleeson, Speg]`

This will output a single random name each time, but always starting with "Mr"

## Referencing generators
Writing complicated generators in one go can be difficult, so instead the task can be split into multiple generators.
One generator can reference the output of another generator by using its name preceded by a `$` symbol, like so:

```
honorific = [Mr, Ms, Mrs, Miss, Master, Sir, Madam]
surname = [Clancey, Dumblethorn, Addlington, Cranch, Asperly]

name = $honorific $surname
```

When the "name" generator is run, it will access the "honorific" and "surname" generators to provide an output (the "honorific" and "surname" generators can also still be run separately).
These generator references can be included in lists and nested.

## Headers

In order to set up some parameters and behaviours globally, an NGen file can include a header, which is surrounded by `^` characters.

```
^
	This is a header
^

gen = This is a generator, it has to come after the header for the header to affect it.
```

A header affects all the generator definitions which follow it, although the behaviours it specifies can be overridden at the generator or at the list level.

### Multiple Headers

An Ngen file can have multiple headers, each one affecting the generator(s) following it. eg:

```
^
	This header affects gen1 and gen2
^
gen1 = [collapsible, pneumatic] [shoulder, parrot]
gen2 = I [came back from, went to] [Tipperary, Anglesey, the pub] 
		with a [chip on my shoulder, small sack of oatmeal]
^
	And this header affects gen3
^
gem3 = [umple, bumple, jigget, splinch]
```

### Setting Pick Type in the header

You can set the Pick Type for all Word Lists in generators which follow a header by writing `pick =` followed by the Pick Type you want from three possibilities: `random`, `shuffle`, and `cycle`, eg:

```
^
	pick = cycle
^

#gen1 and gen2 will both have their Pick Type set to cycle

gen1 = [a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, y, v, x, y, z]
gen2 = [one, two, three, four, five]

^
	pick = shuffle
^

#gen3 will have its Pick Type set to shuffle

gen3 = [ 1 of Hearts, 2 of Clubs, 4 of Diamonds, 3 of Spades, Queen of Hearts, etc ]
```

There is a shorthand way of choosing the Pick Type. Instead of writing `pick = random` the `%` symbol can be used followed by a `r`, an `s`, or a `c` for 'random', 'shuffle', or 'cycle' respectively. 
It is important that there be no whitespace between the two characters, so `%s` is acceptable but `% s` will not work.

```
^
	%c
^

# gen 1 will be set to cycle

gen1 = [Greg, Nancy, Albert, Mario]

^
	%s
^

#gen2 will be set to shuffle

gen2 = [Watermelon, Catapult, Saddle, Joshua]
```

However, for the sake of clarity, it is not recommended to use this format in the header, rather it is intended to be used in other places (as yet not implemented!)

see [Pick Types](#pick-types) for more information.


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

#the dollar-signs should be escaped like this:
money2 = [\$100, \$200]

```
