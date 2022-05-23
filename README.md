# NGen
A procedural text generator orientated towards generating short texts - like names

text generators are written in simple text files

Here are some examples of what is currently possible

### randomly accessed lists of words

Lists are the basic unit of construction for all name generators.
They are contained within square brackets (`[]`) and elements within them are separated by commas (`,`):

`name = [Rupert, Marjory, Caleb, Alba]`

This will output a single name from the list each time

### nested lists

lists can be nested

`name = [[Jason, Alberto, Vince], [Wendy, Samantha, Pauala]]`

This will still output a single name each time, from one or other of the nested lists

### text outside lists

Not all text need be inside a list.
Text outside brackets will always be reproduced

`name = Mr [Thronton, Cleeson, Speg]`

This will output a single random name each time, but always starting with "Mr"
