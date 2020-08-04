// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019-2020, Mozilla.

// Based upon, and extended from: http://wiki.unity3d.com/index.php/Singleton
using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T _instance;

	private static object _lock = new object();

    protected virtual bool PersistOnSceneChange()
    {
        return true;
    }

	protected virtual void Awake()
	{
		lock (_lock) 
		{
			// If there is one of us that is attached to a GameObject, we'll use that one at Awake time, and won't have to go searching for it in the Instance method.
			// This implementation is slightly redundant, but should also be slightly more efficient than handling everything in the Instance method. As such, the Awake implementation could probably be safely jettisoned.
			if (_instance == null) 
			{
				// If I am the first instance, make me the Singleton
				_instance = (T)this;

                _instance.PersistIfNecessary();
			}
			else
			{
				// If a Singleton already exists and you find
				// another reference in scene, destroy it
				if (this != _instance) 
				{
					Debug.Log("[Singleton] '" + typeof(T) + "': There should never be more than 1 singleton! Destroying the extra one");
					Destroy (gameObject);
				}
			}
		}
	}

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting) {
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock(_lock)
			{
				if (_instance == null)
				{
					T[] objects  = FindObjectsOfType (typeof(T)) as T[];
					if (objects.Length == 1) 
					{
						_instance = objects [0];
					}
					else if ( objects.Length > 1 )
					{
                        Debug.Log("[Singleton] '" + typeof(T) + "': There should never be more than 1 singleton! Destroying the extra(s).");
						foreach (T extraObject in objects )
						{
							// Destroy extra instances
							if (extraObject != _instance)
							{
								Destroy (extraObject.gameObject);
							}
						}
					}
					else
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<T>();
						singleton.name = "(singleton) "+ typeof(T);
					}

                    _instance.PersistIfNecessary();
				}

				return _instance;
			}
		}
	}

    private void PersistIfNecessary()
    {
        if (_instance.PersistOnSceneChange())
        {
            // Make sure it is at the root, to avoid DontDestroyOnLoad warning about non-root objects
            _instance.transform.parent = null;
            DontDestroyOnLoad(_instance);
        }
    }
	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it has been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	public virtual void OnDestroy () 
	{
        // We only mark applicationIsQuitting as true, if we are destroying the singleton instance, and it is meant to persist
        if (this == _instance && PersistOnSceneChange())
        {
            applicationIsQuitting = true;
        }
	}
}