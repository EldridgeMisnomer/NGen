# NGen
A simple scripting language for creating procedural text generators with a focus on the creation of short texts - like names.

---
***Note***:

NGen is in active development but is not ready for use, in particular there is no way yet to load your own text files into NGen.

NGen is initially being developed as a console application, but will eventually by a Unity component.

Below you will find documentation for NGen; it is, however, incomplete, erroneous, rambling, and badly organised. This will be fixed, eventually.

---

Text Generators are written in simple text files. At their most basic they consist of a single named Generator (`name = generator`), for example:

```
hi = Hello World
```

An NGen file can contain multiple Generators with complex constructions for returning text.

<details><summary>Table of Contents</summary>

* [Generators](#generators)
* [Generator Components](#generator-components)
	* [Lists](#lists)
    * [Sentences](#sentences)
    * [Proxies](#proxies)
* [Component Settings](#component-settings)
    * [Lists](#list-settings)
    * [Sentences](#sentence-settings)
    * [Proxies](#proxy-settings)
* [Headers](#headers)
* [Tags](#tags)
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

In the above example, every time the Generator is run it will return the same text: "some text".
This is an extremely simple Generator, but much more complex ones can be constructed.

There are three diferent components which Generators can contain:

* [Lists](#lists)
* [Sentences](#sentences)
* [Proxies](#proxies)

 The most important component of most Generators is the [List](#lists)

### Generator Names

A Generator Name must be unique – no two Generators can have the same Name; it can contain any characters with a few exceptions, and no whitespace. 

Generator Names cannot contain these characters: `=`, `$`, `[`, `]`, `(`, `)`, `,`

//TODO - check if this is true - there may be other characters I haven't thought of - especially when it comes to proxies

Names are case insensitive, that is 'Name' is the same as 'name' is the same as 'NAME' is the same as 'nAmE'.

Some example functional and nonfunctional names:

```
# these names are all good:

adj = [ short, wiry, blue ]
PANIC = [ argh, wraaaagh, oof, erk, eeeep ]
so_slow = [ slowly, very slowly, inch by inch, oh so slow]
*!&xx??** = [ !#**, #@!!, @@x!*, !!! ]

# although the last one is perhaps not the most sensible name for a generator

# the following names won't work:

# 'adj' has already been used, names must be unique so this won't work:
adj = [ wobbly, worried, slick ]

# names are case insensitve, and 'adj' has already been used, so this won't work:
Adj = [ stinky, smelly, shrieking ]

# this won't work because it has prohibited characters in it:
*!&$$??** = [ !#**, #@!!, @@x!*, !!! ]

# this won't work because names cannot contain spaces:
long gen = once upon a time in the land of the jolly blue mermen...
# actually, it will work... 
#	but the name of the generator will be 'long' and the 'gen' will be discarded...

```
### Running Generators

Once an NGen has been created from a text file you can generate text with it by running one of the Generators it contains with the `GenTxt( generatorName )` method which run the Generator and return its output as a string:

```
NGen nGen = ???
string output = nGen.GenTxt( "generatorName" );
```

You can get an array containing all the Generator Names using the `GetGenNames()` method:

```
string[] genNames = nGen.GetGenNames();
```

You can get multiple output texts from the same Generator by using the `GenTxt( generatorName, numberOfOutputs )` method, which will return a string array:

```
string[] outputs = nGen.genTxt( "generatorName", 10 );
```

You can get a Generator output using Tags by using the `GenTxt( "generatorName", "tags" )` method, where "tags" can either be a string array of Tag names, or Tags as separate arguments, eg:

*TODO - write this better*

```
string[] tags = { "tag1", "tag2", "tag3" };
string output = nGen.GenTxt( "generatorName", tags );

//Or:

string output = nGen.GenTxt( "generatorName", "tag1", "tag2", tag3" );
```

See [below](#tags) for more about Tags.

#### Indirectly running Generators

*TODO*

### Multiline Generators

Complicated Generators take space to construct and doing so on a single line can be tricky and hard to read; so longer definitions can be written on multiple lines:

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

As we will see later, the Components of Generators ([Lists](#lists), [Sentences](#sentences), and [Proxies](#proxies)) have [Settings](#component-settings) which can be changed.

While each component can have its Settings changed individually, it is also possible to apply Settings to *all* of the components in a Generator by doing it at the Generator level.

Care must be taken when doing so, however, because unexpected results may arise.

Settings in Generators are written in exactly the same way as they are on individual Components but are placed the Generator Name and the `=` sign, for example:

```
gen1 %0.7 _- &f4 = [ one, two, three, four ]
```

They are separated from the generator name by a space and may be written together in one block or separated themselves (`?w0.7&n` or `?w0.7 &n` are both acceptable).

Settings will be applied to only the Components for which they are relevant (ie. you can't set a Separator on a Proxie, or a Once on a Sentence); Settings will be applied to all the Components for which they are relevant (ie. Repeat will be set on both a List and a Proxy).

Here's an example of where this can cause trouble:

```
proxy = three
trickygen &f2 = [ one, $proxy, two ]
```

In the above a Fixed Repeat 2 is set at the Generator level of `trickygen`, this means that elements which can Repeat (Lists and Proxies) will repeat their output twice. However, `trickygen` contains a Proxy inside a List, both of which will repeat.

If only 'one' or 'two' is selected from the List then the output might be "two one one", but if the proxy is selected it will perform its repeat twice, and the output could be "two three three three three three three". If this is what's expected then fine, if it's not then a solution is to set the Repeats on individual Components, either: `trickygen  = &f2[ one, $proxy, two ]`, or `trickygen  = [ one, &f2$proxy, two ]`

For more information about specific Settings, see [Component Settings](#component-settings)

### Main Generators

Although all Generators are created equal, some may be more equal than others, in that you might only be interested in the output of certain Generators, for example:

```
weather = [ sunny, rainy, windy ]
time = [ mornin, afternoon, evening ]

sentence = It was a $weather $time <.
```

In the above, while it might always be possible for me to get the Output from any Generator, in practice maybe I'm only ever interested in the final 'sentence' Generator. As a quality of life feature I can mark it as a Main Generator by preceeding the Name with an `@` symbol, like so: `@sentence = It was a $weather $time <.`. Internally this won't affect how the Generator runs at all, and its name remains 'sentence'.

However I can use the `GetGenNames( true )` method to return the names of only Main Generators instead of all the Generators. I can also use the `GenAll( true )` method to get the Output from all the Main Generators instead of all the Generators.

*TODO - implement this!*

## Generator Components

A Generator can hold three different types of components, any generator will have at least one components, although it may consist of many.

* [Lists](#lists) are the most common and most useful component, they consist of a list of words of which only one will be output by the Generator.
* [Sentences](#sentences) contain multiple elements which will *all* be output in sequence by the Generator.
* [Proxies](#proxies) allow one generator to reference the ouput of another Generator.

### Lists

Lists are the basic unit of construction for most name generators. They are contained within square brackets `[]` and elements within them are separated by commas `,`:

`name = [Rupert, Marjory, Caleb, Alba]`

This will output a single name from the list each time.

For more about customising Lists, see [List Settings](#list-settings)

#### Nested Lists

Lists can be nested, eg:

`name = [[Jason, Alberto, Vince], [Wendy, Samantha, Paula]]`

This will still output a single name each time, from one or other of the nested lists

### Sentences

Most of the time you don't have to think about Sentences, there are no special symbols used for writing them, and they will be created automatically when needed. They also don't have many Behaviours which can be changed.

Sentences consist of multiple elements which will always be output by the Generator they are in, for example:

`name = Mr [Thronton, Cleeson, Speg]`

The text "Mr" above will always be output by the Generator, no matter what. Both the text "Mr" and the List ("Thronton, Cleeson, Speg") are contained within a Sentence which will output both of them in order.

In the example below there are two Lists, both of which will be output, in order, because they are together in a Sentence:

`vp = [run, jog, sprint] [quickly, energetically, furiously]`

For more about Sentences see [Sentence Settings](#sentence-settings)


### Proxies
Writing complicated Generators in one go can be difficult, so instead the task can be split into multiple Generators.

A Proxy is a way of including the output from one Gnerator inside another Generator; it is writting by using a `$` symbol followed by the Generator Name, like so:

```
honorific = [Mr, Ms, Mrs, Miss, Master, Sir, Madam]
surname = [Clancey, Dumblethorn, Addlington, Cranch, Asperly]

name = $honorific $surname
```

In the above example, When the "name" Generator is run, it will take the output from the "honorific" and "surname" Generators and combine them in a Sentence (the "honorific" and "surname" Generators can also still be run separately).

For more about Proxies see [Proxy Settings](#proxy-settings)

## Component Settings

Components have certain Settings which can be changed to alter how they behave. For example, Lists output one random element, the way it chooses this element can be modified by changing its Settings.

Settings are most important for Lists, but all Component types have different Settings.

* [List Settings](#list-settings)
* [Sentence Settings](#sentence-settings)
* [Proxy Settings](#proxy-settings)

Normally the settings for a component are written imediately before that component, not separated by a space, eg:

```
time = [ day, morning, afternoon, evening, saturday ]
gen = It was a ?s[ lovely, wonderful, delightful, gorgeous ] *$time
```

In the above example the List has its Pick Setting set to Shuffle, and the Proxy has it's Once Setting set to On.

Settings can also be set [in the Header](#settings-in-the-header) or [in the Generator](#settings-in-generators). See the applicable sections for more information.

## List Settings

Lists have Settings in the following categories:

* [Pick](#pick-settings)
* [Repeat](#repeat-settings)
* [Output Chance](#output-chance-setting)
* Allow Duplicates - TODO *think of a better name for this*

Settings are normally changed using shorthand codes written either after the Generator Name or immediately before the List:

```
#two different ways of changing a list's settings

#here the settings are written in shorthand after the generator name
gen1 %0.8 &n = [ hat, coat, scarf, gloves ]

#here the settings are written in shorthand before the list
gen2 = ?c&f1[ boots, sandles, slippers ]
```

Settings after a Generator Name are applied to all Lists in the Generator, whereas Settings before a List apply only to that particular List.

Settings after a Generator Name *must* be separated from the Name by a space, and can be written separately or bunched together (ie: either `genname %8 &n = ...` or `genname %8&n= ...`), on the other hand Settings before a List *cannot* be separated from the List by a space and *must* be written bunched together ( ie: `&w[one, two, three]`).

See below for details about how to apply Settings to Lists. TODO - *link this up*


### Pick Settings

Lists always output one element, normally chosen at random; exactly how this word is picked can be changed by modifying a List's Pick Settings.

There are four different ways of picking an element for output: `random`(the default), `shuffle`, `cycle`, and `weighted`.

Pick Settings are applied to a list by using the `?` symbol followed by a letter to indicate which Pick method you want (`?r`, `?s`, `?c`, `?w`), followed by any optional settings you want to apply.

Writing just '?' will set the Pick Settings to their default (`random`), useful when, for example, you have applied a different Pick method after the Generator Name, but you want a List to be set back to `random`, eg:

```
#the generator has pick set to shuffle, this will be set for all the lists in the generator
#the third list has its pick set back to the default - random

gen1 ?s = I put my [ shirt, shoes, socks ] on, shoved my 
	[ keys, wallet, phone ] in my pocket, left the house 
	and jumped ?[ in the car, on my bike, into the jeep ]
```

For more about what the different symbols mean and how to change Settings, see the sections below:

* [Random](#random-pick)
* [Shuffle](#shuffle-pick)
* [Cycle](#cycle-pick)
* [Weighted](#weighted-pick)

#### Random Pick

The default Pick method is `random` - one of the elements from the list will be selected at random each time, every element has an equal probability of being picked each time.

```
randomgen = [harpsichord, lute, harmonium, flute, harp, oboe]
```
Each time the generator is run, any one of the elements in the list has an equal chance of being picked.

There are no optional settings for `random`.

You can set a List to `random` with just the `?` symbol, or by writing `?r`, eg:

```
gen = ?r[ alpha, beta, gamma, delta, epsilon, zeta ]
```

#### Shuffle Pick

When Pick is set to `shuffle`, List elements are still picked randomly, but instead of picking a random one each time, the whole list is reordered randomly before the generator is ever run, and then the elements are picked in order, one at a time. 

When all the elements have been picked, the List is shuffled again and picking starts again at the beginning.
This mostly avoids the same element being picked twice in a row (it can still happen directly after a shuffle), and ensures that all elements from the list are seen given enough picks.

```
shufflegen = ?s[bee, spider, caterpillar, beetle]
```
In the above example, the list might be reordered to `[spider, beetle, caterpillar, bee]` and will then output first 'spider', then 'beetle', then 'caterpillar'. Once 'bee' has been picked the list is shuffled again.

You can set a List to `shuffle` by writing the Pick symbol `?` followed by the letter `s`, as in the above example.

`shuffle` has an optional Shuffle Point Setting. The Shuffle Point is how far through the list you want to shuffle, by default this is set to one, but can be set to any point between 0 and 1 (must be greater than 0) by writing the number you want after the letter `s`.

For example, ` ?s0.5[ one, two, three, four, five, six ]` this List will be shuffled after half the elements have been output from it (in this case 3).

#### Cycle Pick

When Pick is set to `cycle`, elements are not picked randomly at all, but instead in the order they were written in. Once the last element is reached the first will be picked next.

```
cyclegen = ?c[shrub, bush, tree] 
```

The above will always output elements in the same order: 'shrub', then 'bush', then 'tree' then 'shrub' and so on.

You can set a List to `cycle` by writing the Pick symbol `?` followed by the letter `c`, as in the above example.

`cycle` has an optional Skip setting. Skip is how many elements are skipped between outputs, by default this is set to 0. You can set the Skip by writing a positive integer (eg. 1,2,3,4...) after the letter `c`, eg: `?c2`.

If the above example were set to Skip 1, like this `cyclegen = ?c1[shrub, bush, tree]` then the output would be: 'shrub', 'tree', 'bush', 'shrub' and so on. Note that, depending on the number of elements in the List, this could mean that some elements are never output.

I'm not sure why this might be useful, but it is possible.

#### Weighted Pick

The `weighted` Pick method is the most versatile, but also the most verbose to set, and a little tricky to understand. It allows you to apply individual weights to each element in the List to control the probability of it being picked. These weights can be set manually or interpolated using a couple of different methods.

```
weightedgen = ?w[ azalea, begonia, carnation, dahlia, erigeron ]
```

In the above example the first element in the List ('azalea') is the most likely to be picked, the second element is slightly less likely to be picked, and the third even less likely. The last element in the List ('erigeron') will be picked extremely rarely, less than one time in 100.

You can set a List to `weighted` by writing the Pick symbol `?` followed by the letter `w`, as above.

##### Setting Weights

There are a three different ways to set the weights:
* [By setting each weight manually](#setting-weights-manually)
* [By setting the first and last weights](#setting-end-weights)
* [By setting a multiplication factor](#setting-a-factor)

###### Setting Weights Manually

The first method of setting weights, by manually setting a weight for each individual element of the list, is the most precise, and also the most transparent; however, it's also the most verbose, and is inconvenient for long lists.

Weights are set by writing a number for each element in the list, separating numbers with a dash `-`, for example:

```
individualweightsgen = ?w1-2-4-8[ one, two, three, four ]
```

There are four different elements in the above List, and four weights have been set ( `1-2-4-8` ), this means that each weight applies to a single element in the list. In this case the first element is the least likely to be picked, the second is twice as likely as that to be picked, and so on, the fourth element is the most likely to be picked, it will be output 8 times out of 15 or about 53% of the time.

###### Setting End Weights

The second way to specify the weights is to set just the first and last weight, and let NGen interpolate all the others. You do this as above, but only setting 2 numbers, separated by a dash (`-`), the first number is the weight of the first element and the second is the weight of the last element, eg:

```
interpolatedweightsgen = ?w10-1[ one, two, three, four ]
```

There are four different elements in the above List, the first ('one') has a weight of `10` and the last ('four') has a weight of `1`. NGen will calculate the remaining weights by interpolating linearly between the first and last weights. In this case this will generate a set of weights like this: `10-7-4-1`, the first element is therefore the most likely to be picked (about 45% of the time), and the last is the least likely to be picked (about 5% of the time).

Note that if you specify more than 2 weights, but not enough for all elements in the list, then linear interpolation will again be used to fill in the missing weights, for example:

```
missingweightsgen = ?w7-5-2[ one, two, three, four ]
```

In the above Generator three weights have been specified but there are four elements in the List, in cases like these the final weight specified is assumed to be for the final element in the List, and the remaining weights are applied to the other elements, starting from the beginning of the List. In this case the weights are `7-5-?-2`, the third weight is missing and will be interpolated as `3.5`, half way between `5` and `2`, giving weights of: `7-5-3.2-2`. This demonstrates that weights don't have to be whole numbers.

Note that if the weights before and after the missing weights are the same, then all missing weights will be the same, for example:

```
missingweightgen = ?w5-3-1-1[ one, two, three, four, five, six, seven]
```
Here there are four weights and seven elements, the last two weights given are both `1` so *all* the missing weights will also be `1`, the final interpolated set of weights here will be: `5-3-1-1-1-1-1`.

###### Setting A Factor

The third and final way of setting Pick weights, is also the default, and the simplest, but it is also the most abstract which is why it has been left to last; it is just to set a multiplication factor by putting a single number (which can be a decimal) after the `w`, like so:

```
factorweightgen = ?w0.6[ one, two, three, four ]
```

In the above example, because the factor is less than 1, each element will be less likely than the last. Internally NGen sets the first element's weight to an arbitrary high number, for example 10, and then each subsequent weight is calculated by multiplying the previous weight by the factor. Here we would get weights of `10-6-3.6-2.16`. This provides a kind of exponential interpolation.

If the factor is greater than 1, then each element will be more likely to be picked than the last. Internally the first weight is set to an arbitrary low number, and then each subsequent weight is again calculated by multiplying the previous weight by the factor, for example:

```
factorweightgen = ?w2[ one, two, three, four ]
```
Here the weights would be calculated as `1-2-4-8`, the final element is the most likely to be picked (about 53% of the time).

Be careful about setting a high factor, particularly with long lists, as you might end up with only the last element being picked most of the time, and the first elements never being picked. For example, with a factor of 3 and a list 10 elements long the last element will be picked about 67% of the time, and the first element will only be picked about 0.003% of the time.

The default factor is `0.8`, and this can be set just by writing `?w`.

#### Repeat Settings

Using Repeat Settings a List can be told to output more than once, either a fixed number of times, or a random number of times using a couple of different methods. This is useful if, for example, you're generating a person's name and you want to add the possibility of a second or even a third name drawn from the same pool.

```
firstname = &n[ Ethelred, Sibella, Catriona, Joseph, Lackery, Chiquita, Alfred ]
lastname = [ Chalfont, D'Ascoyne, Farquharson, MacCodrun, Parkin, Pendlebury ]

name = $firstname $lastname
```

In the above example, a simple person's name generator, the List in the firstname Generator is set up to repeat up to 4 times, which results in outputs such as: 'Sibella Chalfont', 'Ethelred Alfred Farquharson', 'Alfred Joseph Lackery Ethelred Chalfont', or 'Catriona MacCodrun', however, most of the time it will produce a name with just one first name.

Repeat Settings are set using the `&` symbol, followed by the first letter of the type of Repeat wanted, there are 4 different  Repeat types: `(f)ixed`, `(u)niform`, `(n)ormal`, and `(w)eighted`; this can then be followed by some optional settings, details of which are below. Some example Repeat Settings are: `&f2`, `&n0-10`


**Note:** All numbers here refer to the number of *repetitions* not the number of instances; so 0 repetitions, still mean that a list will output once. To add the possibility of a list not outputting at all, you have to set the [Output Chance Setting](#output-chance-setting).

Repetition can be set in generators or in the header, see [Setting Repeat Type in the Generator](#setting-repeat-type-in-the-generator) or [Setting Repeat Type in the Header](#setting-repeat-type-in-the-header) for more information.

##### Fixed Repeat Type

A List with a `fixed` Repeat Type will always output the same number of times. Currently the default number of repetitions is 0, and there is no way to change it, so this Repeat Type isn't at all useful yet.

##### Uniform Repeat Type

A List with a `uniform` Repeat Type will repeat a number of times between its minimum and maximum (both inclusive). The default minimum is 0 and the default maximum is 3. Whenever the List has to output text it will pick a number between its minimum and maximum following a uniform distribution, that is each possibility has an equal chance to occur, 0 is just as likely as 3.

```
uniformlist &u = [ Henry, Mary, Jane, Louis, Caleb, Martha, Richard, Anna ] 
```

Example outputs might be:
	'Mary Louis Louis', 
	'Henry', 
	'Richard Caleb'
And so on.

##### Normal Repeat Type

A `normal` repeat type will repeat a number of times between its minimum and maximum following a normal or gaussian distribution, with the mean equal to the minimum and the standard deviation calculated so that the maximum is possible but very unlikely (somewhere between 0.1 and 0.01%).
The default minimum is 0 and the default max is 4. This means that 0 is the most likely number of repeats, 1 is less likely than 0, 2 is less likely than 1, 3 is less likely than 2, and 4 is the least likely of all possibilities.

Normal distributions do not typically have a minimum and maximum, however we set them here firstly because negative numbers have no meaning in this context, and secondly to avoid extreme outliers – it is perfectly possible that while 99% of the time there are 0-3 repeats, every so often we'll get 10 repeats, and normally that is not desirable. It is possible however to retain this behaviour by manually setting the standard deviation along with a very high maximum, see below. TODO

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


#### Output Chance Setting

Lists will normally provide an output every time a Generator will run, however they can be given a chance not to do so.

The Output Chance is set using the `%` symbol followed by a number between 0 and 100, a percentage chance for the List to provide an output, for example `%60` gives the List a 60% chance to provide an output, conversely, 40% of the time the List will output nothing.

```
sometimesnothinggen = %90[ one, two, three ]
```

Ten percent of the time, the generator above will not output any text at all.

Proxies can also have an Output Chance.



### Sentence Settings

Sentences only have one Setting - Separator.

### Proxy Settings

Proxies have a number of Settings, most of which are shared with Lists
Proixes have a single setting that is unique to them, called Once.

* [Once](#once-setting)
* Repeat
* [Output Chance](#output-chance)

#### Once Setting

Once can be set to On or Off, default is Off.

The normal use for a Proxy is to place the output from one Generator inside another, thus allowing more complex constructions. Generally the same effect can be achieved using Nested Lists, but Proxies tend to be easier to read and can also be reused in more than one other Generator.

Turning Once on changes this Behaviour. If Once is set to On then the Proxy will only get the output from its Generator Once, the first time it is accessed, and thereafter it will store whatever was output the first time; this allows a Proxy to be used as a kind of storage. Take this example:

```
items = [ wallet, keys, left shoe, favourite tie ]
lostitem * = $items
s1 = Yesterday I lost my $lostitem/. 
	I was so sad, I loved my $lostitem so much, 
	I didn't know what I'd do without my $lostitem/.
```

Here NGen is being used to tell a simple story with a procedurally generated element – the item which is lost is chosen randomly, if the Proxy containing the possible lost items were used normally it would mean that the lost item would change each time it is referenced in the sentence; instead, the Proxy is set to Once and it stores the first item it gets and repeats it every other time it is asked for output.

Once can be switched on by typing the symbol `*`, or off by typing `*!`.






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

### Settings in the Header

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

## Tags

Tags provide another way of organising your generators, at their simplest they don't provide any additional functionality, but they can be more versatile and provide an easier way to access varied output.

Consider this example (without tags):

```
poshdwelling = [ mansion, villa, country house ]
poordwelling = [ hovel, hut, bedsit ]

dwelling = [ $poshdwelling, $poordwelling ]
```

In the above example there are three Generators: 'poshdwelling', which will output an example of a posh place to live, 'poordwelling', which will output a poor place to live, and 'dwelling' which, when you get its output will provide either a poor or a posh place to live.

The same effect can be achieved using tags. Tags allow you to give two or more generators the same name, so long as they have different tags:

```
dwelling (posh) = [ mansion, villa, country house ]
dwelling (poor) = [ hovel, hut, bedsit ]
```

Here, we have just one generator, defined twice with different tags, when you access the output of 'dwelling' it will provide either a posh or a poor place to live, however you can also provide tags when you access generators, which will limit the possibilities to either posh or poor, you do this in the same way as with the `GenTxt( name )` method, but add additional tags: `GenTxt( name, tag1, tag2, tag3 )`. You can add as many tags as you like, although in the above example only two will have any effect: `GenTxt( "dwelling", "posh")` will output a posh place to live, and `GenTxt( "dwelling", "poor")` will output a poor place to live.

Let's look at a more complicated example to see how Tags can be useful, we'll write a name generator with two separate variables - male/female and posh/prole, first without tags:

```
firstnamefemaleposh = [ Dorothea, Elisabeth, Lucinda, Juliet ]
firstnamemaleposh = [ Clarence, Richard, Edwin, Augustus ]
firstnamefemaleprole = [ Dot, Liz, Lucy, Jill ]
firstnamemaleprole = [ Clive, Dick, Ed, Alf ]

lastnameprole = [ Dubbins, Smith, Lapper, Boff ]
lastnameprole = [ Ellington-Dukesbury, Wently-Lefferdale, Dinglington-Bradley, Carlington-Dash ]

femalename = [ $firstnamefemaleposh $lastnameposh, $firstnamefemaleprole $lastnameprole ]
malename = [ $firstnamemaleposh $lastnameposh, $firstnamemaleprole $lastnameprole ]

name = [ $femalename, $malename ]
```

And now let's write the same thing but with tags:

```
firstname (female) (posh) = [ Dorothea, Elisabeth, Lucinda, Juliet ]
firstname (male) (posh) = [ Clarence, Richard, Edwin, Augustus ]
firstname (female) (prole) = [ Dot, Liz, Lucy, Jill ]
firstname (male) (prole) = [ Clive, Dick, Ed, Alf ]

lastname (prole) = [ Dubbins, Smith, Lapper, Boff ]
lastname (posh) = [ Ellington-Dukesbury, Wently-Lefferdale, Dinglington-Bradley, Carlington-Dash ]

name = $firstname $lastname
```

It should be clear that the second version has some advantages: it is more readable for a start, and it requires less intermediate generators without sacrificing the possibility of retrieving the results of those intermetiate generators (we can call `GenTxt( "name", "female" )` and get the equivalent of calling `GenTxt( "femalename" )` in the first example).

Furthermore, what if we wanted to retreive a posh name, without regard for gender? With tags we can call `GenTxt( "name", "posh" )`, but in the first example we would have to write two new Generators:

```
prolename = [ $firstnamefemaleprole, $firstnamemaleprole ] $lastnameprole
poshname = [ $firstnamefemaleposh, $firstnamemaleposh ] $lastnameposh
```

The more complicated the example the greater value tags can have, imagine if we wanted to add names in another language to the above example, with tags we would just have to add some new generators for the new language, while tagging our existing generators with `(english)` or similar, without tags it would require a much more complicated construction.

For a more extensive example of how tags can be used, see the Placename Generator in the Examples folder (*TODO - doesn't exist yet* ).

### How do Tags actually work?

Any Generator can be assigned Tags by writing them after the Generator Name and before the `=` symbol, surrounded by brackets `( )`, like so: `generator name (tag) = [ one, two, three ]`. 

The same [rules which apply to Generator names](#generator-names) also apply to tags (including the fact that they are case-insensitive).

Generators can be given multiple tags, and there are two ways to write these, either: `(tag1, tag2, tag3)` or `(tag1) (tag2) (tag3)`. The second version is prefered, because normally commas `,` in NGen suggest mutually exlusive options in a List, but the first version is offered as a possibility because it is less verbose.

*TODO - decide about this, because the first option would be easier to add a header to*

One Generator with Tags on its own doesn't do anything, for Tags to work you need to have at least two Generators with the same Name and different tags, for example:

```
numbers (arabic) = [ 1, 2, 3, 4, 5 ]
numbers (roman) = [ I, II, III, IV, V ]
```

These two identically named Generators will, internally be combined into one Generator, and when the 'numbers' Generator is run it will output randomly from one or the other.

*TODO - think about if we need to add all the Pick options from Lists to Tags*

This in itself is not particularly useful, however we can also specify a Tag (or Tags) when we access a Generator and get the corresponding result.

The standard way to access a Generator in NGen is to use the `GenTxt( generatorname )` method, to specify Tags when we access a Generator we can include the Tag names after the Generator Name, like so: `GenTxt( "numbers", "roman" )`.

Whenever we access a Generator with Tags it will attempt to provide us with output from a Generator which contains all the Tags we have specified, so if we access the numbers Generator with `GenTxt( "numbers", "arabic" )` we'll receive an output from the List `[ 1, 2, 3, 4, 5 ]`.

If a Generator can't be found with the correct tags, for example if we access `GenTxt( "numbers", "words" )` then Output will be given from one of the available Generators chosen at random.

If we access a Generator with conflicting Tags, for example: `GenTxt( "numbers", "roman", "arabic" )`, then Output will be given from one of the available Generators chosen at random.

If some of the Tags we specify are available and other aren't then the Generator will do its best, for example: `GenTxt( "numbers", "arabic", "big" ) will still give us output from the `arabic` Generator. More precisely the Generator with the most matching Tags will be Output, if there is more than one Generator with most matching Tags, then one of these will be picked at random.

#### Tags and Proxies

All of the above refers to accessing Generators directly, but what happens when Generators are accessed indirectly (via a Proxy)?

Generators with Tags pass their Tags down to other Generators they contain (via Proxies) which in turn provide an output just as if the user had supplied the tags, for example:

```
fname (es) = [ Javier, Ignacio, Carlos, Juan ]
fname (en) = [ Shaun, Richard, Ben, Mike ]
fname (fr) = [ François, Leo, Claude, Louis ]

sen (en) = I once knew a man called $fname <.
```

In the above, the 'sen' Generator has a Proxy which links to the 'fname' Generator. There are 'fname' Generators with three different tags, corresponding to different languages. Because the 'sen' Generator also has a Tag ('en'). Even though 'sen' only has one version, and so the Tag doesn't do anything directly to its output, it will pass this Tag to the 'fname' Generator and so will only make a sentence with an English name.

However, User Tags, those input with `GenTxt( genName, tags )` will override internal Tags, so in the above example, if you generate an output with `GenTxt( "sen", "fr" ), you will receive an output with a French name in it, not the default English one.

This allows you to, for example, set a default type of output for a Generator.

## Comments

Comments can be written along with generators, by starting lines with a `#` symbol:

```
# This is a comment and will be ignored

gen = this is not a comment and will create a generator
```
Every commented line must have a `#` as its first character.


## Escaping Characters

As we have seen, NGen uses special characters like `[` and `$` to define things like Lists and References, the complete list of these special characters is:

`# = [ ] , $ /`

Sometimes you want to use these characters in your text, to do so they need to be escaped by putting a `\` character before them, otherwise they will be read as part of your generator structure, and not part of its content. eg:

```
#these dollar-signs will be read as references to generators named '100' and '200',
#generators which probably don't exist
money = [$100, $200]

#the dollar-signs should be escaped like this:
money2 = [\$100, \$200]

```

### Table of Special Characters

Here is a table of all Special Characters in NGen:

| Character | Function							|
|-----------|-----------------------------------|
| #  		| Comment							|
| =			| assignement						|
| [			| start List						|
| ]			| end List							|
| ,			| separates List elements			|
| $			| denotes a Proxy				    |
| \|		| marks the beginning of a new Sentence |
| >			| indicates no separator between this and the following element |
| <			| indicates no separator between this and the preceding element |

### Table of Shorthand Characters

Here's a table showing all the codes used in shorthand Settings:


| Character | Function			|  Settings			|
|-----------|-------------------|-------------------|
| ?  		| Pick				| ?r, ?s, ?c, ?w    |
| &			| Repeat			| &f, &u, &n, &w	|
| %			| Output Chance		|					|
| _			| Separator			|					|
| *			| Once				|					|
| ~			| Allow Duplicates  |					|
| !			| Switch Off		| *!, ~!			|

## Error Messages

Having a problem with your NGen file? NGen will try to always give you an error when it identifies a problem with your file, below you can find all the errors NGen produces along with explanations and examples.

### Generator Reference Error

#### Full Text:

*"Generator Reference Error: '\{name\}' has not been created"*

#### Explanation:

Generator References allow the output of one Generator to be put into another generator, like so:

```
gen1 = [ a, b, c ]
gen2 = [ $gen1, d, e, f ]
```

In this case, NGen has not been able to find a Generator with the name given in the Reference.

#### Example:

```
#This is INCORRECT code
gen1 = [ a, b, c ]
gen2 = [ $gem1, d, e, f ]
```

#### Possible Solution:

The most likely cause is the misspelling of one or other of the names, either the Generator name or the name in the Reference, as above. To solve it, correct the spelling of one or the other, the above code should read:

```
gen1 = [ a, b, c ]
gen2 = [ $gen1, d, e, f ]
```

### Duplicate Generator Name Error

#### Full Text:

*"Duplicate Generator Name Error: Only the first generator with the name '\{name\}' has been added."*

#### Explanation:

All Generators must have unique names. If two or more generators share a name, then only the first one will be added by NGen, any subsequent ones will be ignored.

#### Example:

```
#This is INCORRECT code
gen = [ a, b, c ]
gen = [ d, e, f ]
```

#### Possible Solution:

Rename all generators with duplicate names.


### Line Processor Error

#### Full Text:

*"Line Processor Error: the number of names (\{names.Count\}) did not match the number of generator declarations (\{declarations.Count\})"*

#### Explanation:

All Generators consist of a name and a declaration - some text to output. This error occurs when NGen identifies either a name without a declaration or a declaration without a name.

#### Example:

```
#This is INCORRECT code
gen =
= [ d, e, f ]
```

#### Possible Solution:

The most likely cause for this problem is a stray `=` which hasn't been escaped, try searching your document for `=` and escaping any which are not part of declarations, like this: `\=`.

Alternatively you may have forgotten to add a name for a generator, or vica-versa.

TODO - Matching Brackets
TODO - Invalid Path
TODO - Character Map does not contain Character
TODO - Number of Repeats with given Mean and StdDev