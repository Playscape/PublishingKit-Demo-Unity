using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Playscape.Internal;
using System;


namespace Playscape.Catalog
{
    public class Catalog
    {
        private PlayscapeCatalogBase mPlayscapeCatalog;

        Catalog()
        {
            GameObject go = GameObject.Find(PlayscapeManager.PLAYSCAPE_MANAGER_GAMEOBJECT_NAME);
            if (go != null)
            {

            #if UNITY_ANDROID && !UNITY_EDITOR
			mPlayscapeCatalog = (PlayscapeCatalogBase)go.GetComponent(typeof(PlayscapeCatalogAndroid));
            #else
            mPlayscapeCatalog = (PlayscapeCatalogBase)go.GetComponent(typeof(PlayscapeCatalogMock));
            #endif
            }
        }

        /// <summary>
        /// The instance.
        /// </summary>
        private static Catalog mInstance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Catalog Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new Catalog();

                    if (mInstance.mPlayscapeCatalog == null)
                    {
                        throw new ApplicationException("Initialization failed - Please place PlayscapeManager prefab into " +
                                                       "the scene and access Instance from onStart() or later (NOT from onAwake()).");
                    }
                }

                return mInstance;
            }
        }


        /// <summary>
        /// show Playscape Exchange catalog.
        /// </summary>
        public void showCatalog()
        {
            mPlayscapeCatalog.showCatalog();
        }
    }
}
