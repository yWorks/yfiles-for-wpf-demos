dp.sh.Brushes.Xml = function()
{
	this.CssClass = 'dp-xml';
	this.Style =	'.dp-xml .cdata { color: #7c6200; }' +
					'.dp-xml .pi { color: black; }' +
					'.dp-xml .text { color: red; }' +
					'.dp-xml .comments { color: #A31515; }' +
					'.dp-xml .tag, .dp-xml .tag-name { color: blue; }' +
					'.dp-xml .attribute { color: green; }' +
					'.dp-xml .attribute-value { color: black; }';
}

dp.sh.Brushes.Xml.prototype	= new dp.sh.Highlighter();
dp.sh.Brushes.Xml.Aliases	= ['xml', 'xhtml', 'xslt', 'html', 'xhtml'];

dp.sh.Brushes.Xml.prototype.ProcessRegexList = function()
{
	function push(array, value)
	{
		array[array.length] = value;
	}
	
	/* If only there was a way to get index of a group within a match, the whole XML
	   could be matched with the expression looking something like that:
	
	   (<!\[CDATA\[\s*.*\s*\]\]>)
	   | (<!--\s*.*\s*?-->)
	   | (<)*(\w+)*\s*(\w+)\s*=\s*(".*?"|'.*?'|\w+)(/*>)*
	   | (</?)(.*?)(/?>)
	*/
	var index	= 0;
	var match	= null;
	var regex	= null;

	// Match processor instructions in the following format <? ... ?>
//	this.GetMatches(new RegExp('(\&lt;|<)\\?[\\s\\S]*\\?(\&gt;|>)', 'gm'), 'pi');
         
	// Match XML declaration in the following format <?xml ... ?>
	this.GetMatches(new RegExp('(\&lt;|<)\\?xml [\\s\\S]*\\?(\&gt;|>)', 'gm'), 'pi');
         
	// Match CDATA in the following format <![ ... [ ... ]]>
	// (\&lt;|<)\!\[[\w\s]*?\[(.|\s)*?\]\](\&gt;|>)
//	this.GetMatches(new RegExp('(\&lt;|<)\\!\\[[\\w\\s]*?\\[(.|\\s)*?\\]\\](\&gt;|>)', 'gm'), 'cdata');
	this.GetMatches(new RegExp('(\&lt;|<)\\!\\[[\\w\\s]*?\\[(.|\\s)*?\\]\\s*\\](\&gt;|>)', 'gm'), 'cdata');
	
	// Match comments
	// (\&lt;|<)!--\s*.*?\s*--(\&gt;|>)
//	this.GetMatches(new RegExp('(\&lt;|<)!--\\s*.*?\\s*--(\&gt;|>)', 'gm'), 'comments');
	this.GetMatches(new RegExp('(\&lt;|<)!--[\\s\\S]*?\\s*--(\&gt;|>)', 'gm'), 'comments');

	// Match attributes and their values
	// (:|\w+)\s*=\s*(".*?"|\'.*?\'|\w+)*
//	regex = new RegExp('([:\\w-\.]+)\\s*=\\s*(".*?"|\'.*?\'|\\w+)*|(\\w+)', 'gm'); // Thanks to Tomi Blinnikka of Yahoo! for fixing namespaces in attributes
	regex = new RegExp('([:\\w-\.]+)\\s*=\\s*("[\\s\\S]*?"|\'[\\s\\S]*?\'|\\w+)*|(\\w+)', 'gm'); // Thanks to Tomi Blinnikka of Yahoo! for fixing namespaces in attributes
	while((match = regex.exec(this.code)) != null)
	{
		if(match[1] == null)
		{
			continue;
		}
			
		push(this.matches, new dp.sh.Match(match[1], match.index, 'attribute'));
	
		// if xml is invalid and attribute has no property value, ignore it	
		if(match[2] != undefined)
		{
			push(this.matches, new dp.sh.Match(match[2], match.index + match[0].indexOf(match[2]), 'attribute-value'));
		}
	}

	// Match text nodes in-between tag brackets
// It's not possible!  this.GetMatches(new RegExp('\\s*([^(\&lt;|<)])+[\\s\\S]*?', 'gm'), 'text');

	// Match opening and closing tag brackets
	// (\&lt;|<)/*\?*(?!\!)|/*\?*(\&gt;|>)
//	this.GetMatches(new RegExp('(\&lt;|<)/*\\?*(?!\\!)|/*\\?*(\&gt;|>)', 'gm'), 'tag');
	this.GetMatches(new RegExp('(\&lt;|<)/*(?!\\!)(?!\\?)|/*(\&gt;|>)', 'gm'), 'tag');

	// Match tag names
	// (\&lt;|<)/*\?*\s*(\w+)
//	regex = new RegExp('(?:\&lt;|<)/*\\?*\\s*([:\\w-\.]+)', 'gm');
	regex = new RegExp('(?:\&lt;|<)/*\\s*([:\\w-\.]+)', 'gm');
	while((match = regex.exec(this.code)) != null)
	{
		push(this.matches, new dp.sh.Match(match[1], match.index + match[0].indexOf(match[1]), 'tag-name'));
	}
}
