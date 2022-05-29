# NGen
A procedural text generator orientated towards generating short texts - like names.

Text generators are written in simple text files. At their simplest they consist of a single named generator, for example:

`hi = Hello World`

An NGen file can contain multiple generators with complex constructions for returning text.

<details><summary>Table of Contents</summary>

* [Generators](#generators)
* [Word Lists](#word-lists)
* [Text outside Lists](#text-outside-lists)
* [Referencing generators](#referencing-generators)
* [Headers](#headers)
* [Comments](#comments)
* [Escaping characters](#escaping-characters)

</details>


---

The following are the currently available features:

## Generators

A generator is a name, followed by an `=` sign, followed by some text which will be returned when that generator is run. 

```
	generator = some text
```

In the above example, every time the generator is run it will return the same text: "some text".
This is an extremely simple generator, but much more complex ones can be constructed. The most important component of most generators is the [List](#word-lists)

### Generator names

A generator name must be unique - no two generators can have the same name; they can have any characters in them except spaces or other whitespace; and they are case insensitive.

### Setting Pick Type in the Generator

The way that words are picked from lists can be set for all lists in a generator (see [Pick Types](#pick-types) for more information on how words are picked from lists).

There are three Pick Types: `random`, `shuffle`, and `cycle`. By writing the Pick Type symbol (`%`) followed by the first letter of the Type you want after the generator name (so `%r`, `%s`, or `%c`), you set the Pick Type for all Lists in that Generator. eg:

```
#this generator will shuffle all its Lists
generator %s = [[Banquo, Rosaline , Bardolph], [Polonius, Oberon, Benvolio], [Demetrius, Fortinbras, Stephano]]
```
The above generator contains multiple, nested lists, all of which will have the shuffle Pick Type, rather than the default Pick Type of random.

__Note:__ The space between the generator name and the Pick Type is one of the few times where whitespace is important in NGen; without the space, the `%s` will be included in the generator name.

Pick Types can also be [set in the header](#setting-pick-type-in-the-header), but setting the pick type at the generator level will override the header's Pick Type.

```
^
	#this pick setting in the header doesn't do anything, 
	#because the generator below has a pick type which will override it.
	pick = shuffle
^
gen1 %c = [a, b, c, d, e, f, g]
```

### Setting Repeat Type in the Generator

TODO

### Multiline generators

Complicated generators take space to construct and doing so on a single line can be tricky and hard to read, so longer definitions can be written on multiple lines

```
multilineVeg = [
	artichoke, beetroot, carrot, cauliflower, celery, 
	endive, kale, leek, marrow, pea, 
	potato, pumpkin, spinach, squash, yarrow
]
```

In general, whitespace such as tabs, linebreaks and spaces, are ignored. There are some exceptions to this, such as every [commented](#comments) line having to have a `#` character at the beginning of it.


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
At the moment, the Pick Type can only be set in the Header (see [Setting Pick Type in the Header](#setting-pick-type-in-the-header)) for more information), and in the generator (see [Setting Pick Type in the Generator](#setting-pick-type-in-the-generator) for more), this will change soon.

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

### Repeats

Lists can be told to repeat themeselves a fixed or random number of times using several different methods for picking the number of repetitions. This is useful if, for example, you're generating a person's name and you want to add the possibility of a second or even a third name drawn from the same pool.

This is done by setting the Repeat Type.
There are 4 different  repeat types: `fixed`, `uniform`, `normal`, and `weighted`.
The default Repeat Type is `fixed`, but the number of repeats are set to zero, so Lists don't, by default, repeat themselves.

**Note:** All numbers here refer to the number of *repetitions* not the number of instances; so 0 repetitions, still mean that a list will output once. To add the possibility of a list not outputting at all, you'll have to wait, because it hasn't been implemented yet.

Repetition can currently only be set in generators, see [Setting Repeat Type in the Generator](#setting-repeat-type-in-the-generator) for more information.

The Repeat symbol is `&` and you choose the type by adding the first letter of the type after the symbol, so: `&f`, `&u`, `&n`, `&w`. If you omit the letter, a `normal` repeat type will be set.

#### Fixed Repeat Type

A List with a `fixed` Repeat Type will always output the same number of times. Currently the default number of repetitions is 0, and there is no way to change it, so this Repeat Type isn't at all useful yet.

#### Uniform Repeat Type

A List with a `uniform` Repeat Type will repeat a number of times between its minimum and maximum (both inclusive). The default minimum is 0 and the default maximum is 3. Whenever the List has to output text it will pick a number between its minimum and maximum following a uniform distribution, that is each possibility has an equal chance to occur, 0 is just as likely as 3.

```
uniformlist &u = [ Henry, Mary, Jane, Louis, Caleb, Martha, Richard, Anna ] 
```

Example outputs might be:
	"Mary Louis Louis", 
	"Henry", 
	"Richard Caleb"
And so on.

#### Normal Repeat Type

A `normal` repeat type will repeat a number of times between its miniumum and maximum following a normal or gaussian distribution, with the mean equal to the minimum and the standard deviation calculated so that the maximum is possible but very unlikely (somewhere between 0.1 and 0.01%).
The default min is 0 and the default max is 4. This means that 0 is the most likely number of repeats, 1 is less likely than 0, 2 is less likely than 1, 3 is less likely than 2, and 4 is the least likely of all possibilities.

**TODO** explain this better when my head is more screwed on, also think of an example below

```
normallist &n = []
```

#### Weighted Repeat Type

**TODO** explain this when my head works at all


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

However, for the sake of clarity, it is not recommended to use this format in the header, rather it is intended to be [used at the generator level](#setting-pick-type-in-generators).

see [Pick Types](#pick-types) for more information.


## Comments

Comments can be written along with generators, by starting lines with a `#` symbol:

```
# This is a comment and will be ignored

gen = this is not a comment and will create a generator
```
Every commented line must have a `#` as its first character.


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
