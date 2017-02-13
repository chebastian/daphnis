# daphnis
A tool for increasing productivity by letting you create and view snippets of your screen that floats above everything else. Easy to use and save snippets for viewing later by tagging. 

## Creating a snippet
* Given you hit the hotkey (Ctrl+F10)
* And drag over what you want to save
* Then your snippet will be created once you release your mouse

##Tagging a snippet
* Given you hover over a snippet
* Or has just created a snippet
* When you write text it should be directed to the tags field
* Then hit enter to add the tag to the snippet

##Move snippet
* Given you click and drag a snippet
* Then the snippet will move with your mouse

##Change Snippet alpha
* Given you hoover over you snippet
* When you scroll you mose
* Then the alpha should increase or decrease depending on direction scrolled

##Saving snippets
* Given you open the snippet overlay (Ctrl+F10)
* And click the topright button save
* Then your open snippets will be saved

##Repoen Saved Snippet
* Given you have opened the snippet overlay
* When you search in the field marked "Find with tag"
* Then any snippet tagged with similar tags should be open

##Closing snippets
* Given you have a snippet opne
* When you dubbelclick it or hit the close button
* Then the snippet will close
* And unless you already saved it wont be saved to your history
