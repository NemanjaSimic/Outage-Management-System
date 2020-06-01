using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FTN.ESI.SIMES.CIM.Manager;


namespace FTN.ESI.SIMES.CIM.Model.Utils
{
	/// <summary>
	/// Class used to generate codeDOM trees from Profile, write files and compile them to DLL
	/// </summary>
    public class CodeDOMUtil
    {
        protected const string MultiplicityZeroOrOneString = "M:0..1";
        protected const string MultiplicityExactlyOneString1 = "M:1..1";
        protected const string MultiplicityExactlyOneString2 = "M:1";
        protected const string MultiplicityZeroOrMoreString = "M:0..n";
        protected const string MultiplicityOneOrMoreString = "M:1..n";

        private const string separator = StringManipulationManager.SeparatorSharp;

        public CodeDOMUtil(string type) 
        {
            defaultNS = type;
        }

        #region FIELDS

        private List<CodeCompileUnit> files = new List<CodeCompileUnit>();

        private List<CodeCompileUnit> Files
        {
            get
            {
                return files;
            }
            set
            {
                files = value;
            }
        }


        /// <summary>
        /// Provider needed for dynamic creating and compiling <c>profile</c> classes
        /// </summary>
        private CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

        /// <summary>
        /// Default namespace for created classes
        /// </summary>
        private string defaultNS = "RDF";

		public string DefaultNS
		{
			get
			{
				return defaultNS;
			}
			set
			{
				defaultNS = value;
			}
		}

        #endregion

        #region DELEGATES AND EVENTS

        /// <summary>
        /// Delegate for messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public delegate void MessageEventHandler(object sender, string message);

        /// <summary>
        /// Event for messages
        /// </summary>
        public event MessageEventHandler Message;

        protected virtual void OnMessage(string message)
        {
            if(Message != null)
                Message(this, message);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Generates classes and enumetarions (CodeCompileUnit) from <c>files</c> list  as C# classes 
        /// </summary>
        public void GenerateCode(Profile profile)
        {
            OnMessage("\r\n\t--------------Generating code--------------");
            List<ClassCategory> list = new List<ClassCategory>();
            List<ProfileElement> prof = new List<ProfileElement>();
            profile.ProfileMap.TryGetValue(ProfileElementTypes.ClassCategory, out prof);
            list = prof.Cast<ClassCategory>().ToList();
            Files.Clear();
            if(list != null)
            {
                //in each package create class and all connected to it - package is  c# namespace
                foreach (ClassCategory package in list)
                {
                    OnMessage("\r\n Package:\t" + package.Name);
                    List<ProfileElement> classes = package.MembersOfClassCategory;
                    if(classes != null)
                    {
                        //FOREACH KLASS IN PACKAGE
                        foreach(ProfileElement entity in classes)
                        {
                            CodeCompileUnit unit = BuildCodeCompileUnit(profile, package, entity);
							if (unit != null)
							{
								//add it to the list, unless its null
								//null value appears when its type is not class

								Files.Add(unit);
							}
                        }
                    }
                    OnMessage("\r\n");
                }

            }
            CreateParentClass();

            OnMessage("\r\n\t----------Done generating code----------");

        }

        /// <summary>
        /// Writes CodeCompileUnits from list <c>files</c> to folder "./classes/" under their respective names
        /// in files representing C# classes
        /// </summary>
        public void WriteFiles(string assemblyVersion)
        {
            OnMessage("\r\n\t--------------Writing files--------------");
            int counter = 0;
            foreach(CodeCompileUnit unit in Files)
            {
                if(unit != null)
                {
					if (unit.Namespaces[0].Types[0].Name.Equals("IDClass"))
					{
						CodeTypeReference attr = new CodeTypeReference("AssemblyVersion");
						CodeAttributeDeclaration decl = new CodeAttributeDeclaration(attr, new CodeAttributeArgument(new CodePrimitiveExpression(assemblyVersion)));
						unit.AssemblyCustomAttributes.Add(decl);
					}
                    String sourceFile;
                    if(provider.FileExtension[0] == '.')
                    {
                        sourceFile = ".\\classes\\" + unit.Namespaces[0].Types[0].Name + provider.FileExtension;
                    }
                    else
                    {
                        sourceFile = ".\\classes\\" + unit.Namespaces[0].Types[0].Name + "." + provider.FileExtension;
                    }
                    if(!System.IO.Directory.Exists(".\\classes\\"))
                    {
                        System.IO.Directory.CreateDirectory(".\\classes\\");
                    }
                    IndentedTextWriter tw = new IndentedTextWriter(new StreamWriter(sourceFile, false), "    ");
                    provider.GenerateCodeFromCompileUnit(unit, tw, new CodeGeneratorOptions());
                    tw.Close();
                    counter++;
                }
            }
            OnMessage("\r\nTOTAL UNITS:" + counter);
            OnMessage("\r\n\t----------Done writing files----------");
        }

        /// <summary>
        /// Method compiles all class files from "./classes/" folder generated by this process
        /// </summary>
        public void CompileCode(string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
            {
                OnMessage("\r\nWARNING: missing file name.");
                return;
            }
            if(Files.Count == 0)
            {
                OnMessage("\r\nWARNING: No classes generated! Nothing to compile.");
                return;
            }
            OnMessage("\r\n\t-------------Compiling code-------------");

            List<string> paths = new List<string>();
            foreach(CodeCompileUnit unit in Files)
            {
                if(unit != null)
                {
					String sourceFile = ".\\classes\\" + unit.Namespaces[0].Types[0].Name + ".cs";
					paths.Add(sourceFile);
                }
            }

			string currentContent = String.Empty;
			string filePath = ".\\classes\\IDClass.cs";
			string newContent = "using System.Reflection;\nusing System.Runtime.CompilerServices;\nusing System.Runtime.InteropServices;\n";
			if (File.Exists(filePath))
			{
				currentContent = File.ReadAllText(filePath);
			}
			File.WriteAllText(filePath, newContent + currentContent);


            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.OutputAssembly = ".\\DLL\\" + fileName + ".dll";
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.TreatWarningsAsErrors = false;

            if(!System.IO.Directory.Exists(".\\DLL\\"))
            {
                System.IO.Directory.CreateDirectory(".\\DLL\\");
            }

            CompilerResults cr = provider.CompileAssemblyFromFile(parameters, paths.ToArray());

            if(cr.Errors.Count > 0)
            {
                OnMessage("\r\n Errors encountered while building \r\n\r\n");
                foreach(CompilerError ce in cr.Errors)
                {
                    OnMessage(ce.ToString() + "\r\n");
                }
                OnMessage("\r\nTotal errors: " + cr.Errors.Count);
            }
            else
            {
                OnMessage("\r\nSource built into with no errors.");

            }
            OnMessage("\r\n\t----------Done compiling code----------");
        }

        #endregion 

        #region support methods

        /// <summary>
        /// Method that creates CodeCompileUnit from the <paramref name="entity"/> parameter.
        /// </summary>
        /// <param name="package">ProfileElement element that represents the package</param>
        /// <param name="entity">ProfileElement element that represents (usualy) class or enumeration</param>
        /// <returns>CodeCompileUnit unit that will be added to files list, for future compiling or writing to a file</returns>
        private CodeCompileUnit BuildCodeCompileUnit(Profile profile, ProfileElement package, ProfileElement en)
        {
            CodeCompileUnit unit = null;

            if (ExtractSimpleNameFromResourceURI(en.Type).Equals("Class"))
            {
                Class entity = (Class) en;
                if (entity.IsEnumeration)
                {
                    //enumeration
                    OnMessage("\r\n\t Enumeration:\t" + entity.Name);
                    unit = CreateEnumeration(package, entity);
                }
                else
                {
                    //class
                    OnMessage("\r\n\t Class:\t\t" + entity.Name);
                    unit = CreateClass(profile, package, entity);
                }
            }
            else
            {
                OnMessage("\r\n\tNot a class or enumeration:" + en.Name);
            }
            return unit;
        }

        private string ExtractSimpleNameFromResourceURI(string resourceUri)
        {
            return StringManipulationManager.ExtractShortestName(resourceUri, StringManipulationManager.SeparatorSharp);
        }

        /// <summary>
        /// Method creates CodeCompileUnit that represents enumeration
        /// NOTE: in this method if character '-' is found it is replaced by string "MINUS"
        /// so that the member field names in CodeCompileUnit ca be valid for C# code
        /// e.g. Hz-1 will become HzMINUS1
        /// </summary>
        /// <param name="package">ProfileElement element that represents the package</param>
        /// <param name="entity">ProfileElement element that represents enumeration</param>
        /// <returns>CodeCompileUnit unit representing enumeration</returns>
        private CodeCompileUnit CreateEnumeration(ProfileElement package, Class entity)
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            //namespace
            string fullNS = package.Name;

            ProfileElement temp = package;

            //CodeNamespace nameSpace = new CodeNamespace(fullNS);
            CodeNamespace nameSpace = new CodeNamespace(defaultNS);
            unit.Namespaces.Add(nameSpace);

            //namespace imports
            nameSpace.Imports.Add(new CodeNamespaceImport("System"));

            CodeTypeDeclaration enumeration = new CodeTypeDeclaration(entity.Name);
            enumeration.IsEnum = true;
            enumeration.Attributes = MemberAttributes.Public;
            enumeration.TypeAttributes = TypeAttributes.Public;

            if(entity.MyEnumerationMembers != null)
            {
                foreach(ProfileElement enumMember in entity.MyEnumerationMembers)
                {
                    string value = enumMember.Name;
                    value = StringManipulationManager.ReplaceInvalidEnumerationCharacters(value);
                    CodeMemberField val = new CodeMemberField(typeof(int), value);
                    if (!string.IsNullOrEmpty(enumMember.Comment))
                    {
                        val.Comments.Add(new CodeCommentStatement(enumMember.Comment, true));
                    }
                    enumeration.Members.Add(val);
                }
            }
            nameSpace.Types.Add(enumeration);
            return unit;
        }

        /// <summary>
        /// Method creates CodeCompileUnit that represents class
        /// </summary>
        /// <param name="package">ProfileElement element that represents the package</param>
        /// <param name="entity">ProfileElement element that represents class</param>
        /// <returns>CodeCompileUnit unit representing class</returns>
        private CodeCompileUnit CreateClass(Profile profile, ProfileElement package, Class entity)
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            //namespace
            string fullNS = package.Name;
            ProfileElement temp = package;
            CodeNamespace nameSpace = new CodeNamespace(defaultNS);
            unit.Namespaces.Add(nameSpace);
			
            //namespace imports
            nameSpace.Imports.Add(new CodeNamespaceImport("System"));

            //class
            CodeTypeDeclaration file = new CodeTypeDeclaration();
            file.IsClass = true;
            file.Name = entity.Name;
            file.TypeAttributes = TypeAttributes.Public;
            file.Attributes = MemberAttributes.Public;

            //parent class... adding namespace also
            if(entity.SubClassOfAsObject != null)
            {
                file.BaseTypes.Add(new CodeTypeReference(entity.SubClassOfAsObject.Name));

                ProfileElement tempParent = entity.SubClassOfAsObject;
                nameSpace.Imports.Add(new CodeNamespaceImport(defaultNS));
            }
            else
            {
                //if class doesn't have a parent,
                //it should extend IDClass as the root of hierarhy - for rdf:ID

                file.BaseTypes.Add(new CodeTypeReference("IDClass"));
                nameSpace.Imports.Add(new CodeNamespaceImport(defaultNS));

            }


            if (!string.IsNullOrEmpty(entity.Comment))
            {
                file.Comments.Add(new CodeCommentStatement(entity.Comment, true));
            }
            
            string dataType = "";

            List<Property> fields = new List<Property>();
            if (entity.MyProperties != null)
            {
                fields = entity.MyProperties.Cast<Property>().ToList();
            }

            if(fields != null)
            {
                foreach (Property field in fields)
                {
                    CodeMemberField att = null;
                    
                    dataType = field.DataType;

                    //if data type is null... it is probably a reference
                    if(field.IsPropertyDataTypeSimple)
                    {
                        //SIMPLE VALUE
                        dataType = field.DataTypeAsSimple.ToString();
                            
                        //multiplicity
                        if (ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityExactlyOneString1) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityExactlyOneString2) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityZeroOrOneString))
                        {
                            //create field
                            string fieldName = "cim_" + field.Name;

                            att = new CodeMemberField((String.Compare(dataType,"system.string",true)==0)?(dataType):(dataType + "?"), fieldName);
								
                            att.Attributes = MemberAttributes.Private;
                            if(!string.IsNullOrEmpty(field.Comment))
                            {
                                att.Comments.Add(new CodeCommentStatement(field.Comment, true));
                            }
                            file.Members.Add(att);                                

                            //property for the field
							CreatePropertyForField(file, dataType, att, true, true);
                            CreateIsMandatoryFieldAndProperty(file, att, field);
                            CreateFieldPrefix(file, att, field);
                        }
                        //when multiplicity is more then 1 we have to add List
                        if (ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityOneOrMoreString) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityZeroOrMoreString))
                        {
                            //import list namespace
                            nameSpace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));

                            string fieldName = "cim_" + field.Name;

                            att = new CodeMemberField(new CodeTypeReference("List",new CodeTypeReference[]{new CodeTypeReference(dataType) }),fieldName);
                            att.Attributes = MemberAttributes.Private;
                            att.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(dataType) }));
                            if(!string.IsNullOrEmpty(field.Comment))
                            {
                                att.Comments.Add(new CodeCommentStatement(field.Comment, true));
                            }
                            file.Members.Add(att);

							CreatePropertyForField(file, att, true, true);
                            CreateIsMandatoryFieldAndProperty(file, att, field);
                            CreateFieldPrefix(file, att, field);
                        }
                    }
                    else
                    {
                        /*
                            * if range as object is empty, we should look for dataType, coz that is where 
                            * data regarding that can be found...
                            * range itself is actually set
                            * 
                            */

                        //REFERENCE - not a simple value
                        string typeName = "";
                        ProfileElement temp1 = null;
                        //in case when both are null - u have to set at least one...
                        //if not say its an error
                        if(field.RangeAsObject == null && field.DataTypeAsComplexObject == null)
                        {
                            if(!string.IsNullOrEmpty(field.Range))
                            {
                                //find what element is it and place it in RangeAsObject
                                temp1 = profile.FindProfileElementByUri(field.Range);
                                if (temp1 != null)
                                {
                                    field.RangeAsObject = temp1;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            //if range was nonexistent and we still need temp 
                            if(temp1 == null)
                            {
                                if(!string.IsNullOrEmpty(field.DataType))
                                {
                                    temp1 = profile.FindProfileElementByUri(field.DataType);
                                    if (temp1 != null)
                                    {
                                        field.DataTypeAsComplexObject = temp1;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                        }

                        if(field.RangeAsObject != null)
                        {
                            if (ExtractSimpleNameFromResourceURI(field.RangeAsObject.Type).Equals("Class"))
                            {
                                Class cl = (Class) field.RangeAsObject;
                                string pack = cl.BelongsToCategoryAsObject != null ? cl.BelongsToCategoryAsObject.Name : "global";
                                typeName = cl.Name;
                                temp1 = cl;
                            }
                            if (ExtractSimpleNameFromResourceURI(field.RangeAsObject.Type).Equals("ClassCategory"))
                            {
                                ClassCategory cl = (ClassCategory)field.RangeAsObject;
                                string pack = cl.BelongsToCategoryAsObject != null ? cl.BelongsToCategoryAsObject.Name : "global";
                                typeName = cl.Name;
                                temp1 = cl;
                            }
                        }
                        if(field.DataTypeAsComplexObject != null)
                        {
                            if (ExtractSimpleNameFromResourceURI(field.DataTypeAsComplexObject.Type).Equals("Class"))
                            {
                                Class cl = (Class)field.DataTypeAsComplexObject;
                                string pack = cl.BelongsToCategoryAsObject != null ? cl.BelongsToCategoryAsObject.Name : "global";
                                typeName = cl.Name;
                                temp1 = cl;
                            }
                        }
                        if(temp1 == null)
                        {
                            OnMessage("\r\nERROR - missing reference, or invalid URI on property:" + field.UniqueName + "in class:" + entity.UniqueName);
                            OnMessage("\r\nProcess canceled!");
                        }
                        
                        //multiplicity check
                        if (ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityZeroOrOneString) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityExactlyOneString1) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityExactlyOneString2))
                        {
                            //creating field 
                            string fieldName = "cim_" + field.Name;								
								
							Class type = null;
							if(field.DataTypeAsComplexObject != null)
							{
								type = (Class)field.DataTypeAsComplexObject;
							}
							if(field.RangeAsObject != null)
							{
								type = (Class)field.RangeAsObject;
							}
							if(type != null && type.IsEnumeration)
							{
								att = new CodeMemberField(typeName + "?", fieldName);
								att.Attributes = MemberAttributes.Private;

								if(!string.IsNullOrEmpty(field.Comment))
								{
									att.Comments.Add(new CodeCommentStatement(field.Comment, true));
								}

								file.Members.Add(att);

								//property for the field
								CreatePropertyForEnumField(file, att, typeName, true, true);
							}
							else
							{
								att = new CodeMemberField(typeName, fieldName);
								att.Attributes = MemberAttributes.Private;

								if(!string.IsNullOrEmpty(field.Comment))
								{
									att.Comments.Add(new CodeCommentStatement(field.Comment, true));
								}

								file.Members.Add(att);

								//property for the field
								CreatePropertyForField(file, att, true, true);
							}
                            CreateIsMandatoryFieldAndProperty(file, att, field);
                            CreateFieldPrefix(file, att, field);
                        }
                        //when multiplicity is more then 1 we have to add List
                        if (ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityOneOrMoreString) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityZeroOrMoreString))
                        {
                            //import List namespace
                            nameSpace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));


                            string fieldName = "cim_" + field.Name;

                            att = new CodeMemberField(new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(typeName) }), fieldName);
                            att.Attributes = MemberAttributes.Private;
                            att.InitExpression = new CodeObjectCreateExpression(new CodeTypeReference("List", new CodeTypeReference[] { new CodeTypeReference(typeName) }));
                                
                            if(!string.IsNullOrEmpty(field.Comment))
                                att.Comments.Add(new CodeCommentStatement(field.Comment, true));
                            file.Members.Add(att);

                            CreatePropertyForField(file, att, true, true);
                            CreateIsMandatoryFieldAndProperty(file, att, field);
                            CreateFieldPrefix(file, att, field);
                        }
                    }
                }
            }

            nameSpace.Types.Add(file);
            return unit;
        }

        private void CreateIsMandatoryFieldAndProperty(CodeTypeDeclaration file, CodeMemberField att, Property field)
        {
            CodeMemberField fieldIsMandatory = new CodeMemberField();
            fieldIsMandatory.Attributes = MemberAttributes.Private | MemberAttributes.Const;
            fieldIsMandatory.Type = new CodeTypeReference(typeof(bool));
            fieldIsMandatory.Name = "is" + StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4)) + "Mandatory";

            //switch case and set true or false
            if (ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityExactlyOneString1) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityExactlyOneString2) || ExtractSimpleNameFromResourceURI(field.MultiplicityAsString).Equals(MultiplicityOneOrMoreString))
            {
                fieldIsMandatory.InitExpression = new CodePrimitiveExpression(true);
            }
            else
            {
                fieldIsMandatory.InitExpression = new CodePrimitiveExpression(false);
            }
            file.Members.Add(fieldIsMandatory);

            CodeMemberProperty propIsMandatory = new CodeMemberProperty();
            propIsMandatory.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            propIsMandatory.Type = new CodeTypeReference(typeof(bool));
            propIsMandatory.Name = "Is" + StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4)) + "Mandatory";
            propIsMandatory.HasGet = true;
            propIsMandatory.HasSet = false;
            propIsMandatory.GetStatements.Add(new CodeSnippetExpression("return " + fieldIsMandatory.Name));

            file.Members.Add(propIsMandatory);

        }

		#region PROPERTY methods

		private void CreatePropertyForEnumField(CodeTypeDeclaration file, CodeMemberField att, string typeName, bool get, bool set)
		{
			CodeMemberProperty prop = new CodeMemberProperty();
			prop.Attributes = MemberAttributes.Public;
			prop.Type = new CodeTypeReference(typeName);
			prop.Name = StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4));
			if(prop.Name.Equals(file.Name))
			{
				prop.Name = prop.Name + "P";
			}
			if(get)
			{

				prop.HasGet = true;
				prop.GetStatements.Add(new CodeSnippetExpression("return this." + att.Name + ".GetValueOrDefault()"));
			}
			if(set)
			{
				prop.HasSet = true;
				prop.SetStatements.Add(new CodeSnippetExpression("this." + att.Name + " = value"));
			}
			file.Members.Add(prop);

			CreateHasValueProperty(file, att);
		}

		private void CreatePropertyForField(CodeTypeDeclaration file, string dataType, CodeMemberField att, bool get, bool set)
		{
			CodeMemberProperty prop = new CodeMemberProperty();
			prop.Attributes = MemberAttributes.Public;

			prop.Type = new CodeTypeReference(dataType);
			prop.Name = StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4));
			if(prop.Name.Equals(file.Name))
			{
				prop.Name = prop.Name + "P";
			}
			if(get)
			{

				prop.HasGet = true;
				prop.GetStatements.Add(new CodeSnippetExpression("return this." + att.Name + ((String.Compare(dataType,"system.string",true)==0)?"":".GetValueOrDefault()")));
			}
			if(set)
			{
				prop.HasSet = true;
				prop.SetStatements.Add(new CodeSnippetExpression("this." + att.Name + " = value"));
			}
			file.Members.Add(prop);

			CreateHasValueProperty(file, att);
		}

		

        /// <summary>
        /// Method creates property field for member field <c>att</c> inside <c>file</c> class
        /// </summary>
        /// <param name="file">CodeTypeDeclaration class that property is added to</param>
        /// <param name="att">CodeMemberField member field that property is made for</param>
        /// <param name="get">bool value - if <c>true</c> get will be added to property field</param>
        /// <param name="set">bool value - if <c>true</c> set will be added to property field</param>
        private static void CreatePropertyForField(CodeTypeDeclaration file, CodeMemberField att, bool get, bool set)
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Attributes = MemberAttributes.Public;

			prop.Type = att.Type;
			prop.Name = StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4));
			if(prop.Name.Equals(file.Name))
			{
				prop.Name = prop.Name + "P";
			}
            if(get)
            {
				
                prop.HasGet = true;
				prop.GetStatements.Add(new CodeSnippetExpression("return this." + att.Name));
            }
            if(set)
            {
                prop.HasSet = true;
                prop.SetStatements.Add(new CodeSnippetExpression("this." + att.Name + " = value"));
            }
            file.Members.Add(prop);

			CreateHasValueProperty(file, att);
        }
        /// <summary>
        /// Method creates constant field and its property which contains prefix of the property 
        /// </summary>
        /// <param name="file">CodeTypeDeclaration class that field and its property  are been added to</param>
        /// <param name="att">CodeMemberField member attribute that prefix field and its property are made for</param>
        /// <param name="field">Property which is being processing</param>
        private void CreateFieldPrefix(CodeTypeDeclaration file, CodeMemberField att, Property field)
        {
            CodeMemberField fieldPrefix = new CodeMemberField();
            fieldPrefix.Attributes = MemberAttributes.Private | MemberAttributes.Const;
            fieldPrefix.Type = new CodeTypeReference(typeof(string));
            fieldPrefix.Name = "_" + att.Name.Substring(4) + "Prefix";
            //set prefix
            if (field.GetUndefinedStereotypes() != null && field.GetUndefinedStereotypes().Count > 0)
            {
                fieldPrefix.InitExpression = new CodePrimitiveExpression(StringManipulationManager.ExtractShortestName(field.GetUndefinedStereotypes().ElementAt(0).Name, separator));
            }
            else
            {
                fieldPrefix.InitExpression = new CodePrimitiveExpression("cim");
            }
            
            file.Members.Add(fieldPrefix);

            CodeMemberProperty propFieldPrefix = new CodeMemberProperty();
            propFieldPrefix.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            propFieldPrefix.Type = new CodeTypeReference(typeof(string));
            propFieldPrefix.Name = StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4)) + "Prefix";
            propFieldPrefix.HasGet = true;
            propFieldPrefix.HasSet = false;
            propFieldPrefix.GetStatements.Add(new CodeSnippetExpression("return " + fieldPrefix.Name));

            file.Members.Add(propFieldPrefix);

        }        


        private static void CreateHasValueProperty(CodeTypeDeclaration file, CodeMemberField att)
        {
            CodeMemberProperty propHasValue = new CodeMemberProperty();
            propHasValue.Attributes = MemberAttributes.Public;

            propHasValue.Type = new CodeTypeReference(typeof(bool));
            propHasValue.Name = StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4)) + "HasValue";
            propHasValue.HasGet = true;
            propHasValue.HasSet = false;
            propHasValue.GetStatements.Add(new CodeSnippetExpression("return this." + att.Name + " != null"));
            file.Members.Add(propHasValue);
        }

		#endregion

		/// <summary>
        /// Method creates predefined class that contains ID and other needed fields
        /// </summary>
        private void CreateParentClass()
        {

            CodeCompileUnit unit = new CodeCompileUnit();
            //namespace
            CodeNamespace nameSpace = new CodeNamespace(defaultNS);
            unit.Namespaces.Add(nameSpace);

            //namespace imports
            nameSpace.Imports.Add(new CodeNamespaceImport("System"));

            //class
            CodeTypeDeclaration file = new CodeTypeDeclaration();
            file.IsClass = true;
            file.Name = "IDClass";
            file.TypeAttributes = TypeAttributes.Public;

            //create field
            string fieldName = "cim_ID";
            CodeMemberField att = new CodeMemberField(typeof(string), fieldName);
            att.Attributes = MemberAttributes.Private;
            att.Comments.Add(new CodeCommentStatement("ID used for reference purposes", true));

            file.Members.Add(att);

            //create property
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Attributes = MemberAttributes.Public;
            prop.Type = new CodeTypeReference(typeof(string));
            prop.Name = "ID";
            prop.HasGet = true;
            prop.GetStatements.Add(new CodeSnippetExpression("return this." + fieldName));
            prop.HasSet = true;
            prop.SetStatements.Add(new CodeSnippetExpression("this." + fieldName + " = value"));

            file.Members.Add(prop);

            CodeMemberField fieldIsMandatory = new CodeMemberField();
            fieldIsMandatory.Attributes = MemberAttributes.Private | MemberAttributes.Const;
            fieldIsMandatory.Type = new CodeTypeReference(typeof(bool));
            fieldIsMandatory.Name = "is" + StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4)) + "Mandatory";

            //switch case and set true or false
            fieldIsMandatory.InitExpression = new CodePrimitiveExpression(true);

            file.Members.Add(fieldIsMandatory);

            CodeMemberProperty propIsMandatory = new CodeMemberProperty();
            propIsMandatory.Attributes = MemberAttributes.Public;
            propIsMandatory.Type = new CodeTypeReference(typeof(bool));
            propIsMandatory.Name = "Is" + StringManipulationManager.CreateHungarianNotation(att.Name.Substring(4)) + "Mandatory";
            propIsMandatory.HasGet = true;
            propIsMandatory.HasSet = false;
            propIsMandatory.GetStatements.Add(new CodeSnippetExpression("return " + fieldIsMandatory.Name));

            file.Members.Add(propIsMandatory);

            nameSpace.Types.Add(file);

            Files.Add(unit);
        }

        #endregion
    }
}
