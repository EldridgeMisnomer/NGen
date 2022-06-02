# NGen
A simple scripting language for creating procedural text generators with a focus on the creation of short texts - like names.

Text generators are written in simple text files. At their simplest they consist of a single named Generator, for example:

`hi = Hello World`

An NGen file can contain multiple Generators with complex constructions for returning text.

<details><summary>Table of Contents</summary>

* [Generators](#generators)
* [Generator Components](#generator-components)
	* [Lists](#lists)
    * [Sentences](#sentences)
    * [Proxies](#proxies)
* [Headers](#headers)
* [Comments](#comments)
* [Escaping characters](#escaping-characters)
* [Error Messages](#error-messages)

</details>


---


## Generators

A Generator consists of a Name, followed by an `=` sign, followed by some text which will be returned when that Generator is run. 

```
	generator = some text
```

In the above example, every time the generator is run it will return the same text: "some text".
This is an extremely simple generator, but much more complex ones can be constructed.

There are three diferent components which Generators can contain:
* [Lists](#lists)
* Sentences
* Proxies

 The most important component of most generators is the [List](#word-lists)

### Generator Names

A Generator Name must be unique – no two Generators can have the same Name; it can contain any characters but no whitespace. 

Names are case insensitive, that is 'Name' is the same as 'name' is the same as 'NAME'.

Some example good and bad names:

```
#these names are all good
adj = [ short, wiry, blue]
PANIC = [ argh, wraaaagh, oof, erk, eeeep ]
so_slow = [ slowly, very slowly, inch by inch, oh so slow]
*!&xx??** = [ !#**, #@!!, @@x!*, !!! ]
#although the last one is perhaps not the most sensible name for a generator

#these names won't work
#'adj' has already been used, names must be unique so this won't work
adj = [ wobbly, worried, slick ]
#names are case insensitve, and 'adj' has already been used, so this won't work
Adj = [ stinky, smelly, shrieking ]
#this won't work because names cannot contain spaces
long gen = once upon a time in the land of the jolly blue mermen...
#(actually, it will work - but the name of the generator will be 'long' and the 'gen' will be discarded...
#...this can be dangerous however, as we will see later, there is a chance that it won't be discarded 
#and will affect the behaviour of the generator

```

### Multiline Generators

Complicated Generators take space to construct and doing so on a single line can be tricky and hard to read, so longer definitions can be written on multiple lines:

```
multilineVeg = [
	artichoke, beetroot, carrot, cauliflower, celery, 
	endive, kale, leek, marrow, pea, 
	potato, pumpkin, spinach, squash, yarrow
]
```


### Mulitiple Generators

A single NGen file can contain definitions for multiple Generators, as many as you like.

```
gen1 = [ a, b, c...
gen2 = [ robot, fish, monster...
gen3 = [ ball, hoop, socket, laughter...
etc
```

### Settings in Generators

As we will see later, the components of Generators (Lists, Sentences, and Proxies) have Behaviours which can be changed with Settings.

Settings in Generators are written in a kind of shorthand between the Generator Name and the `=` sign, for example:

```
gen1 %0.7 _- &f4 = [ one, two, three, four ]
```

These will be covered in detail below

TODO - link to relevant sections once they exist

**Note:** The space between the Generator Name and the Settings is important, without it the Settings will be ignored and will be included in the Name.

## Generator Components

A Generator can hold three different types of components, any generator will have at least one components, although it may consist of many.

* [Lists](#lists) are the most common and most useful component, they consist of a list of words of which only one will be output by the Generator.
* [Sentences](#sentences) contain multiple elements which will *all* be output in sequence by the Generator.
* [Proxies](#proxies) allow one generator to reference the ouput of another Generator.

### Lists

Lists are the basic unit of construction for most name generators. They are contained within square brackets `[]` and elements within them are separated by commas `,`:

`name = [Rupert, Marjory, Caleb, Alba]`

This will output a single name from the list each time.

#### Nested Lists

Lists can be nested, eg:

`name = [[Jason, Alberto, Vince], [Wendy, Samantha, Paula]]`

This will still output a single name each time, from one or other of the nested lists

#### Pick Types

The word which is selected for output from a list can be picked in one of three basic ways: random, shuffle, and cycle.
At the moment, the Pick Type can only be set in the Header (see [Setting Pick Type in the Header](#setting-pick-type-in-the-header)) for more information), and in the generator (see [Setting Pick Type in the Generator](#setting-pick-type-in-the-generator) for more), this will change soon.

##### Random pick type

The default Pick Type is random - one of the elements from the list will be selected at random each time.

```
^
	pick = random
^

randomgen = [harpsichord, lute, harmonium, flute, harp, oboe]
```
Each time the generator is run, any one of the elements in the list has an equal chance of being picked

##### Shuffle pick type

When the Pick Type is set to shuffle, elements are still picked randomly, but instead of picking a random one each time, the whole list is reordered randomly before the generator is ever run, and then the elements are picked in order, one at a time. When all the elements have been picked, the list is reordered randomly again and picking starts again at the beginning.
This mostly avoids the same element being picked twice in a row (it can still happen directly after a shuffle), and ensures that all elements from the list are seen given enough picks.

```
^
	pick = shuffle
^

shufflegen = [bee, spider, caterpillar, beetle]
```
In the above example, the list might be reordered to `[spider, beetle, caterpillar, bee]` and will then output first 'spider', then 'beetle', then 'caterpillar'. Once 'bee' has been picked the list is shuffled again.

##### Cycle pick type

When the Pick Type is set to cycle, elements are not picked randomly at all, but instead in the order they were written in. Once the last element is reached the first will be picked next.

```
^
	pick = cycle
^

cyclegen = [shrub, bush, tree] 
```
The above will always output elements in the same order: 'shrub', then 'bush', then 'tree' then 'shrub' and so on.

#### Repeats

Lists can be told to repeat themselves a fixed or random number of times using several different methods for picking the number of repetitions. This is useful if, for example, you're generating a person's name and you want to add the possibility of a second or even a third name drawn from the same pool.

This is done by setting the Repeat Type.
There are 4 different  Repeat Types: `fixed`, `uniform`, `normal`, and `weighted`.
The default Repeat Type is `fixed`, but the number of repeats are set to zero, so Lists don't, by default, repeat themselves.

**Note:** All numbers here refer to the number of *repetitions* not the number of instances; so 0 repetitions, still mean that a list will output once. To add the possibility of a list not outputting at all, you'll have to wait, because it hasn't been implemented yet.

Repetition can be set in generators or in the header, see [Setting Repeat Type in the Generator](#setting-repeat-type-in-the-generator) or [Setting Repeat Type in the Header](#setting-repeat-type-in-the-header) for more information.

##### Fixed Repeat Type

A List with a `fixed` Repeat Type will always output the same number of times. Currently the default number of repetitions is 0, and there is no way to change it, so this Repeat Type isn't at all useful yet.

##### Uniform Repeat Type

A List with a `uniform` Repeat Type will repeat a number of times between its minimum and maximum (both inclusive). The default minimum is 0 and the default maximum is 3. Whenever the List has to output text it will pick a number between its minimum and maximum following a uniform distribution, that is each possibility has an equal chance to occur, 0 is just as likely as 3.

```
uniformlist &u = [ Henry, Mary, Jane, Louis, Caleb, Martha, Richard, Anna ] 
```

Example outputs might be:
	"Mary Louis Louis", 
	"Henry", 
	"Richard Caleb"
And so on.

##### Normal Repeat Type

A `normal` repeat type will repeat a number of times between its minimum and maximum following a normal or gaussian distribution, with the mean equal to the minimum and the standard deviation calculated so that the maximum is possible but very unlikely (somewhere between 0.1 and 0.01%).
The default min is 0 and the default max is 4. This means that 0 is the most likely number of repeats, 1 is less likely than 0, 2 is less likely than 1, 3 is less likely than 2, and 4 is the least likely of all possibilities.

**TODO** explain this better when my head is more screwed on, also think of an example below

```
normallist &n = []
```

##### Weighted Repeat Type

A `weighted` List will repeat a number of times between zero and a chosen maximum, but allows you to assign weights to each possible number to make it more or less likely.
The default weights are: `{ 3, 4, 2, 1 }`, the first number in the list representing the chance for zero repeats, the last number in the list representing the chance for the maximum number of repeats (in this example 3), and the nth number in the list representing the chance for that number of repeats.

So, for example, with weights of { 0, 1, 0, 1, 0, 1 } a List will always repeat at least once and it could repeat either 1, 3, or 5 times, with each of those numbers having an equal probability.

Or, another example, the weights: { 1, 2, 1 } allow a list to repeat either 0, 1, or 2 times, but once is twice as likely as the other two possibilities.

This Repeat Type gives the greatest flexibility in assigning probabilities to number of repeats, but it is also the most verbose and the fiddliest to use.

**Note:** Currently there is no way to change the weights. This will be amended in future.

```
weightedgen &w = I was [ very ] happy.
```
In the above example there is only one list and it only contains one word, "happy", because the list is weighted it can repeat up to 3 times, to produce one of the following sentences:

	I was very happy.
	I was very very happy.
	I was very very very happy.
	I was very very very very happy.

However, the second sentence is the most likely, the first is slightly less like, the third is half as likely as the second, and the fourth is the least likey - 4 times less likely than the second.


### Sentences

Not all text need be inside a list. Text outside of lists will always be reproduced, eg:

`name = Mr [Thronton, Cleeson, Speg]`

This will output a single random name each time, but always starting with "Mr"

### Proxies
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
gen3 = [umple, bumple, jigget, splinch]
```

### Setting Behaviours in the Header

The primary use for the header is to set the behaviour of all generators which follow it.
Currently Pick Type, Repeat Type and No Repeat behaviour can be set, there is also a `reset` command which will switch everything back to its defaults.


#### Setting Pick Type in the Header

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

see [Pick Types](#pick-types) for more information.


#### Setting Repeat Type in the Header

you can set the Repeat Type for all Lists which follow a header by writing `repeat =` followed by the Repeat Type you want, there are four possibilities: `fixed`, `uniform`, `normal`, and `weighted`.

There is currently no way of changing any of the settings for these Repeat Types - they stay on their defaults. This is to come soon.

```
^
	repeat = weighted
^

#gen will repeat following a weighted distribution
gen = [ ho, humm, zoodle, wordle ]
```

see [Repeats](#repeats) for more information.


#### Resetting behaviour in the Header

Header settings can be reset to their defaults by using the keyword `reset`.

When using multiple headers, settings in a second header will overwrite those in the first, however, any settings present in the first but not in the second will not be changed, eg:

```
^
	repeat = cycle
	pick = shuffle
^

gen1 = [ one, two...

^
	pick = random
^

gen2 = [ one, two...
```

In the above example, because the second header doesn't set the Repeat Type, `gen2` will have a Repeat Type of `cycle`, which was set in the first header.

In order to change all settings back to their defaults, use the `reset` keyword somewhere in the header, as below where `gen2` will have the default Repeat Type of `fixed`.

```
^
	repeat = cycle
	pick = shuffle
^

gen1 = [ one, two...

^
	reset
	pick = random
^

gen2 = [ one, two...
```

The location of `reset` in the header is not important.`

A header can contain just the word `reset` in order to change everything back to their defaults without setting any other behaviours.


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

## Error Messages

Having a problem with your NGen file? NGen will try to always give you an error when it identifies a problem with your file, below you can find all the errors NGen produces along with explanations and examples.

### Generator Reference Error

##### Full Text:

*"Generator Reference Error: '\{name\}' has not been created"*

##### Explanation:

Generator References allow the output of one Generator to be put into another generator, like so:

```
gen1 = [ a, b, c ]
gen2 = [ $gen1, d, e, f ]
```

In this case, NGen has not been able to find a Generator with the name given in the Reference.

##### Example:

```
#This is INCORRECT code
gen1 = [ a, b, c ]
gen2 = [ $gem1, d, e, f ]
```

##### Possible Solution:

The most likely cause is the misspelling of one or other of the names, either the Generator name or the name in the Reference, as above. To solve it, correct the spelling of one or the other, the above code should read:

```
gen1 = [ a, b, c ]
gen2 = [ $gen1, d, e, f ]
```

### Duplicate Generator Name Error

##### Full Text:

*"Duplicate Generator Name Error: Only the first generator with the name '\{name\}' has been added."*

##### Explanation:

All Generators must have unique names. If two or more generators share a name, then only the first one will be added by NGen, any subsequent ones will be ignored.

##### Example:

```
#This is INCORRECT code
gen = [ a, b, c ]
gen = [ d, e, f ]
```

##### Possible Solution:

Rename all generators with duplicate names.


### Line Processor Error

##### Full Text:

*"Line Processor Error: the number of names (\{names.Count\}) did not match the number of generator declarations (\{declarations.Count\})"*

##### Explanation:

All Generators consist of a name and a declaration - some text to output. This error occurs when NGen identifies either a name without a declaration or a declaration without a name.

##### Example:

```
#This is INCORRECT code
gen =
= [ d, e, f ]
```

##### Possible Solution:

The most likely cause for this problem is a stray `=` which hasn't been escaped, try searching your document for `=` and escaping any which are not part of declarations, like this: `\=`.

Alternatively you may have forgotten to add a name for a generator, or vica-versa.

TODO - Matching Brackets
TODO - Invalid Path
TODO - Character Map does not contain Character
TODO - Number of Repeats with given Mean and StdDev