using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SyncLib
{
	//This class reads the defined config section (if available) and stores it locally in the static _Config variable.  
	//This config data is available by calling MedGroups.GetMedGroups().
	//public class SageCompanies
	//{
	//	public static SageCompanyConfigSection _Config = ConfigurationManager.GetSection("SageCompanyConfig") as SageCompanyConfigSection;

	//	public static SageElementCollection GetSageCompanies()
	//	{
	//		return _Config.SageCompanies;
	//	}
	//}

	//Extend the ConfigurationSection class.  Your class name should match your section name and be postfixed with "Section".
	public class SageCompanyConfigSection : ConfigurationSection
	{
		//Decorate the property with the tag for your collection.
		[ConfigurationProperty("SageCompanies")]
		public SageElementCollection SageCompanies
		{
			get { return (SageElementCollection)this["SageCompanies"]; }
		}
	}

	//Extend the ConfigurationElementCollection class.
	//Decorate the class with the class that represents a single element in the collection.	
	[ConfigurationCollection(typeof(SageElement))]
	public class SageElementCollection : ConfigurationElementCollection
	{
		public SageElement this[int index]
		{
			get { return (SageElement)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
					BaseRemoveAt(index);

				BaseAdd(index, value);
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new SageElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SageElement)element).Name;
		}
	}

	public class SageElement : ConfigurationElement
	{
		[ConfigurationProperty("Name", DefaultValue = "", IsRequired = true)]
		public string Name
		{
			get { return (string)this["Name"]; }
			set { this["Name"] = value; }
		}

		[ConfigurationProperty("Prefix", IsRequired = true)]
		//[StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;’\"|\\", MinLength = 3, MaxLength = 3)]
		public string Prefix
		{
			get { return (string)this["Prefix"]; }
			set { this["Prefix"] = value; }
		}
	}
}
