using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO;
using CIM.Model;
using CIM.Manager;

namespace CIMParser
{
    public class ProfileLoader
    {

        #region FIELDS

        /// <summary>
        /// profile with the information
        /// </summary>
        private Profile profile;

        /// <summary>
        /// List that contains <typeparamref name="ProfileElement"/> elements that are referenced in
        /// <c>profile</c> classes, but not defined. This list represents elements that will be
        /// completed with the information aquired from the EAP model of the standard.
        /// </summary>
        private List<ProfileElement> predefined = new List<ProfileElement>();

        private List<ProfileElement> newPredefined = new List<ProfileElement>();

        #endregion


        /// <summary>
        /// Delegate for messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public delegate void MessageEventHandler(object sender, string message);

        /// <summary>
        /// Delegate for done parsing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="model"></param>
        public delegate void DoneParsingEventHandler(object sender, Profile profile);


        /// <summary>
        /// Parsing finished event
        /// </summary>
        public event DoneParsingEventHandler DoneParsing;

        /// <summary>
        /// Event for messages
        /// </summary>
        public event MessageEventHandler Message;


        protected virtual void OnMessage(string message)
        {
            if(Message != null)
                Message(this, message);
        }

        protected virtual void OnDoneParsing(Profile profile)
        {
            if(profile != null)
            {
                DoneParsing(this, profile);
            }
        }

        #region Support Methods

        private void addNewPredefined()
        {
            profile.GetAllProfileElementsOfType(ProfileElementTypes.Class).AddRange(newPredefined);
            newPredefined.Clear();
        }

        /// <summary>
        /// Adds missing data to <c>entity</c> with respect to <c>modelClass</c>, and <c>package</c> from the model
        /// </summary>
        /// <param name="entity">ProfileElement element with missing data</param>
        /// <param name="modelClass">CClass from model</param>
        /// <param name="package">CPackage from model. Contains the <c>modelClass</c></param>
        private void updateClassData(ProfileElement entity, CClass modelClass, CPackage package)
        {
            entity.AddStereotype(ProfileElementStereotype.StereotypeConcrete);
            entity.URI = modelClass.name;
            //PACKAGE: name and object - added "Package_" because of the current scheme
            entity.BelongsToCategory = "Package_" + package.name;
            entity.TypeAsEnumValue = ProfileElementTypes.Class;
            entity.Label = StringManipulationManager.ExtractAllAfterSeparator(modelClass.name, StringManipulationManager.SeparatorSharp);
            List<ProfileElement> packages = profile.GetAllProfileElementsOfType(ProfileElementTypes.ClassCategory);
            foreach(ProfileElement pack in packages)
            {
                if(pack.UniqueName.Equals(entity.BelongsToCategory))
                {
                    entity.BelongsToCategoryAsObject = pack;
                    pack.AddToMembersOfClassCategory(entity);
                    break;
                }

            }
            //if package is not found, look for it in the model
            //-----------------------------------------------------------------------------


            if(entity.BelongsToCategoryAsObject == null)
            {
                //looking for package (and we know which one - CPackage) and adding it to hierarchy
                ProfileElement newPackage = new ProfileElement();
                newPackage.Label = package.name;
                newPackage.URI = "Package_" + package.name;

                //create separate package - detached from hierarchy, because we wont need it
                newPackage.TypeAsEnumValue = ProfileElementTypes.ClassCategory;
                newPackage.AddToMembersOfClassCategory(entity);
                entity.BelongsToCategoryAsObject = newPackage;
                entity.BelongsToCategory = newPackage.UniqueName;
                profile.GetAllProfileElementsOfType(ProfileElementTypes.ClassCategory).Add(newPackage);
            }

            //-----------------------------------------------------------------------------

            //PARENT: assumption is that all the classes needed are in profile, 
            //we dont need to add additional, not mentioned in profile
            entity.SubClassOf = modelClass.parentClassName == null ? string.Empty : modelClass.parentClassName;
            if(!string.IsNullOrEmpty(entity.SubClassOf))
            {
                bool foundInModel = false;
                List<ProfileElement> classes = profile.GetAllProfileElementsOfType(ProfileElementTypes.Class);
                foreach(ProfileElement temp in classes)
                {
                    if(temp.URI.Equals(entity.SubClassOf))
                    {
                        entity.SubClassOfAsObject = temp;
                        break;
                    }
                }
                if(!foundInModel)
                {
                    entity.SubClassOf = string.Empty;
                }
            }

            //FIELDS
            foreach(CAttribute att in modelClass.attributes)
            {
                addAttributeToElement(entity, att, modelClass);
            }
        }

        /// <summary>
        /// Adds information about attribure <c>att</c> to <c>entity</c>
        /// </summary>
        /// <param name="entity">ProfileElement to which attribute is being added</param>
        /// <param name="att">CAttribute contains information about attribute</param>
        private void addAttributeToElement(ProfileElement entity, CAttribute att, CClass modelClass)
        {
            ProfileElement newProperty = new ProfileElement();
            newProperty.URI = att.name;
            newProperty.DataType = att.type;

            if(ProfileElementStereotype.StereotypeEnumeration.Equals(modelClass.stereotype))
            {
                newProperty.DataType = "int";
                newProperty.TypeAsEnumValue = ProfileElementTypes.EnumerationElement;
                entity.AddToMyEnumerationMembers(newProperty);
            }
            else
            {
                newProperty.TypeAsEnumValue = ProfileElementTypes.Property;
                if(!newProperty.IsPropertyDataTypeSimple)
                {
                    List<ProfileElement> list = new List<ProfileElement>();
                    list.AddRange(profile.GetAllProfileElementsOfType(ProfileElementTypes.Class));
                    list.AddRange(predefined);
                    list.AddRange(newPredefined);
                    //if it is not simple, then it is a reference in profile, or another missing one?
                    foreach(ProfileElement type in list)
                    {
                        if(type.UniqueName.Equals(att.type))
                        {
                            //if its from profile, add needed references
                            newProperty.DataTypeAsComplexObject = type;
                            break;
                        }
                    }
                    if(newProperty.DataTypeAsComplexObject == null)
                    {
                        //not found so it has to be added in next iteration
                        //create flawed element so that it can be found
                        //flawed in a way that it only has URI
                        ProfileElement newElement = new ProfileElement();
                        newElement.URI = att.type;
                        newPredefined.Add(newElement);
                        newProperty.DataTypeAsComplexObject = newElement;
                    }
                }
                entity.AddToMyProperties(newProperty);
            }

        }


        /// <summary>
        /// Searches through the profile, determining which elements are not complete and adds them to
        /// <c>predefined</c> list.
        /// </summary>
        private void extractEmptyClasses()
        {
            List<ProfileElement> elList = new List<ProfileElement>();
            elList = profile.GetAllProfileElementsOfType(ProfileElementTypes.Class);
            if(elList != null)
            {
                if(elList.Count > 0)
                {
                    foreach(ProfileElement e in elList)
                    {
                        if(string.IsNullOrEmpty(e.BelongsToCategory))
                            predefined.Add(e);
                    }
                }
            }
            OnMessage("\r\nPredefined classes count:" + predefined.Count);
        }

        #endregion

        #region URI checks

        /// <summary>
        /// Method checks if URI is embeded in prefix (base) in xml, and if it is, adds it to places that need it
        /// for easier use later.
        /// </summary>
        /// <param name="profile">Profile profile that is being checked</param>
        public void addMissingURIParts(Profile profile)
        {

            List<ProfileElement> list = new List<ProfileElement>();
            profile.ProfileMap.TryGetValue(ProfileElementTypes.ClassCategory, out list);
            //PACKAGES
            if(list != null)
            {
                //IN EACH PACKAGE
                foreach(ProfileElement package in list)
                {


                    //check URI
                    if(package.URI.StartsWith("#"))
                    {
                        package.URI = profile.BaseNS + package.URI;
                    }

                    List<ProfileElement> classes = package.MembersOfClassCategory;
                    if(classes != null)
                    {
                        foreach(ProfileElement elem in classes)
                        {
                            if(elem.TypeAsEnumValue == ProfileElementTypes.Class)
                            {
                                if(elem.URI.StartsWith("#"))
                                {
                                    elem.URI = profile.BaseNS + elem.URI;
                                }
                                checkPropertiesURI(elem, profile);
                                checkEnumerationURI(elem, profile);
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Checks and adds missing URI parts to enumeration members
        /// </summary>
        /// <param name="elem">ProfileElement enumeration that is being checked</param>
        /// <param name="profile">Profile profile that is being checked</param>
        private void checkEnumerationURI(ProfileElement elem, Profile profile)
        {
            if(elem.MyEnumerationMembers != null)
            {

                foreach(ProfileElement enumMember in elem.MyEnumerationMembers)
                {
                    if(enumMember.URI.StartsWith("#"))
                    {
                        enumMember.URI = profile.BaseNS + enumMember.URI;
                    }
                }
            }
        }

        /// <summary>
        /// Checks and adds missing URI parts to properties
        /// </summary>
        /// <param name="elem">ProfileElement class whose properties are being checked</param>
        /// <param name="profile">Profile profile that is being checked</param>
        private void checkPropertiesURI(ProfileElement elem, Profile profile)
        {
            if(elem.MyProperties != null)
            {
                foreach(ProfileElement property in elem.MyProperties)
                {
                    if(property.URI.StartsWith("#"))
                    {
                        property.URI = profile.BaseNS + property.URI;
                    }
                }
            }
        }

        #endregion

    }
}
