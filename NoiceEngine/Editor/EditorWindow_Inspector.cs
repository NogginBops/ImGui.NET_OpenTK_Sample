using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Engine.Components.Renderers;
using ImGuiNET;

namespace Engine;

public class EditorWindow_Inspector : EditorWindow
{
	private string addComponentPopupText = "";

	private GameObject selectedGameObject;
	private Material selectedMaterial;
	private int contentMaxWidth;
	public static EditorWindow_Inspector I { get; private set; }

	public override void Init()
	{
		I = this;
	}

	public override void Update()
	{
	}

	public void OnGameObjectSelected(int id)
	{
		if (id == -1)
		{
			selectedGameObject = null;
		}
		else
		{
			selectedGameObject = Scene.I.GetGameObject(id);

			selectedMaterial = null;
		}
	}

	public void OnMaterialSelected(string materialPath)
	{
		selectedMaterial = MaterialCache.GetMaterial(Path.GetFileName(materialPath)); //MaterialAssetManager.LoadMaterial(materialPath);

		selectedGameObject = null;
	}

	public override void Draw()
	{
		if (active == false)
		{
			return;
		}

		windowWidth = 800;
		contentMaxWidth = windowWidth - (int) ImGui.GetStyle().WindowPadding.X * 1;
		ImGui.SetNextItemWidth(windowWidth);
		ImGui.SetNextWindowPos(new Vector2(Window.I.ClientSize.X, 0), ImGuiCond.Always, new Vector2(1, 0));
		//ImGui.SetNextWindowBgAlpha (0);
		ImGui.Begin("Inspector", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
		ResetID();

		if (selectedGameObject != null)
		{
			ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
			DrawGameObjectInspector();
			ImGui.PopStyleVar(1);
		}

		if (selectedMaterial != null)
		{
			DrawMaterialInspector();
		}

		ImGui.End();
	}

	private void DrawMaterialInspector()
	{
		PushNextID();
		string materialName = Path.GetFileNameWithoutExtension(selectedMaterial.path);
		ImGui.Text(materialName);

		ImGui.Text("Shader");
		float itemWidth = 200;
		ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth);
		ImGui.SetNextItemWidth(itemWidth);

		string shaderPath = selectedMaterial.shader?.path ?? "";
		string shaderName = Path.GetFileName(shaderPath);
		bool clicked = ImGui.Button(shaderName, new Vector2(ImGui.GetContentRegionAvail().X, 20));
		if (clicked)
		{
			EditorWindow_Browser.I.GoToFile(shaderPath);
		}

		if (ImGui.BeginDragDropTarget())
		{
			ImGui.AcceptDragDropPayload("SHADER", ImGuiDragDropFlags.None);

			shaderPath = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
			if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && shaderPath.Length > 0)
			{
				//shaderPath = Path.GetRelativePath("Assets", shaderPath);
				Shader shader = new Shader(shaderPath);

				selectedMaterial.shader = shader;
				MaterialAssetManager.SaveMaterial(selectedMaterial);
			}

			ImGui.EndDragDropTarget();
		}

		if (selectedMaterial.shader != null)
		{
			ShaderUniform[] shaderUniforms = selectedMaterial.shader.GetAllUniforms();
			for (int i = 0; i < shaderUniforms.Length; i++)
			{
				PushNextID();

				ImGui.Text(shaderUniforms[i].name);

				if (shaderUniforms[i].type == typeof(Vector4))
				{
					ImGui.SameLine(ImGui.GetWindowWidth() - 200 - 5);
					ImGui.SetNextItemWidth(itemWidth);

					if (selectedMaterial.shader.uniforms.ContainsKey(shaderUniforms[i].name) == false)
					{
						continue;
					}

					object uniformValue = selectedMaterial.shader.uniforms[shaderUniforms[i].name];
					System.Numerics.Vector4 col = ((Vector4) uniformValue).ToNumerics();

					if (ImGui.ColorEdit4("", ref col))
					{
						//selectedMaterial
						int lastShader = ShaderCache.shaderInUse;
						ShaderCache.UseShader(selectedMaterial.shader);

						selectedMaterial.shader.SetColor(shaderUniforms[i].name, col);
						ShaderCache.UseShader(lastShader);
					}
				}

				if (shaderUniforms[i].type == typeof(float))
				{
					ImGui.SameLine(ImGui.GetWindowWidth() - 200 - 5);
					ImGui.SetNextItemWidth(itemWidth);

					if (selectedMaterial.shader.uniforms.ContainsKey(shaderUniforms[i].name) == false)
					{
						selectedMaterial.shader.uniforms[shaderUniforms[i].name] = Activator.CreateInstance(shaderUniforms[i].type);
					}

					object uniformValue = selectedMaterial.shader.uniforms[shaderUniforms[i].name];
					float fl = ((float) uniformValue);

					if (ImGui.InputFloat("xxx", ref fl))
					{
						//selectedMaterial
						int lastShader = ShaderCache.shaderInUse;
						ShaderCache.UseShader(selectedMaterial.shader);

						selectedMaterial.shader.SetFloat(shaderUniforms[i].name, fl);
						ShaderCache.UseShader(lastShader);
					}
				}
			}
		}
	}

	private void DrawGameObjectInspector()
	{
		if (selectedGameObject.isPrefab)
		{
			if (ImGui.Button("Update prefab"))
			{
				Serializer.I.SaveGameObject(selectedGameObject, selectedGameObject.prefabPath);
			}

			if (ImGui.Button("Delete prefab"))
			{
				selectedGameObject.isPrefab = false;
			}
		}

		PushNextID();
		ImGui.SetScrollX(0);

		string gameObjectName = selectedGameObject.name;
		ImGui.Checkbox("", ref selectedGameObject.activeSelf);
		ImGui.SameLine();
		PushNextID();
		ImGui.SetNextItemWidth(contentMaxWidth);
		if (ImGui.InputText("", ref gameObjectName, 100))
		{
			selectedGameObject.name = gameObjectName;
		}

		for (int componentIndex = 0; componentIndex < selectedGameObject.components.Count; componentIndex++)
		{
			Component currentComponent = selectedGameObject.components[componentIndex];
			PushNextID();

			//ImGui.SetNextItemWidth (300);
			ImGui.Checkbox("", ref currentComponent.enabled);
			ImGui.SameLine();

			if (ImGui.Button("-"))
			{
				selectedGameObject.RemoveComponent(currentComponent);
				continue;
			}

			ImGui.SameLine();
			PushNextID();

			if (ImGui.CollapsingHeader(currentComponent.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
			{
				FieldOrPropertyInfo[] infos;
				{
					FieldInfo[] _fields = currentComponent.GetType().GetFields();
					PropertyInfo[] properties = currentComponent.GetType().GetProperties();
					infos = new FieldOrPropertyInfo[_fields.Length + properties.Length];
					List<Type> inspectorSupportedTypes = new List<Type>
					                                     {
						                                     typeof(GameObject),
						                                     typeof(Material),
						                                     typeof(Vector3),
						                                     typeof(Vector2),
						                                     typeof(Texture),
						                                     typeof(Color),
						                                     typeof(bool),
						                                     typeof(float),
						                                     typeof(int),
						                                     typeof(string),
						                                     typeof(List<GameObject>),
						                                     typeof(Action),
						                                     typeof(AudioClip),
					                                     };
					for (int fieldIndex = 0; fieldIndex < _fields.Length; fieldIndex++)
					{
						infos[fieldIndex] = new FieldOrPropertyInfo(_fields[fieldIndex]);
						if (_fields[fieldIndex].GetValue(currentComponent) == null)
						{
							//infos[fieldIndex].canShowInEditor = false;
						}
					}

					for (int propertyIndex = 0; propertyIndex < properties.Length; propertyIndex++)
					{
						infos[_fields.Length + propertyIndex] = new FieldOrPropertyInfo(properties[propertyIndex]);
						if (properties[propertyIndex].GetValue(currentComponent) == null)
						{
							infos[_fields.Length + propertyIndex].canShowInEditor = false;
						}
					}

					for (int infoIndex = 0; infoIndex < infos.Length; infoIndex++)
						if (inspectorSupportedTypes.Contains(infos[infoIndex].FieldOrPropertyType) == false)
						{
							infos[infoIndex].canShowInEditor = false;
						}
				}
				for (int infoIndex = 0; infoIndex < infos.Length; infoIndex++)
				{
					FieldOrPropertyInfo info = infos[infoIndex];
					Type fieldOrPropertyType = info.FieldOrPropertyType;
					if (info.canShowInEditor == false)
					{
						continue;
					}

					PushNextID();

					bool hovering = false;
					if (ImGui.IsMouseHoveringRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new System.Numerics.Vector2(1500, ImGui.GetFrameHeightWithSpacing())))
					{
						hovering = true;
					}

					ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);

					if (hovering)
					{
						ImGui.TextColored(Color.Plum.ToVector4(), info.Name);
					}
					else
					{
						ImGui.Text(info.Name);
					}

					float itemWidth1 = 400;
					ImGui.SameLine(ImGui.GetWindowWidth() - itemWidth1);
					ImGui.SetNextItemWidth(itemWidth1);

					if (fieldOrPropertyType == typeof(Vector3))
					{
						System.Numerics.Vector3 systemv3 = (Vector3) info.GetValue(currentComponent);
						if (ImGui.DragFloat3("", ref systemv3, 0.01f))
						{
							info.SetValue(currentComponent, (Vector3) systemv3);
						}
					}
					else if (fieldOrPropertyType == typeof(Vector2))
					{
						System.Numerics.Vector2 systemv2 = (Vector2) info.GetValue(currentComponent);
						if (ImGui.DragFloat2("", ref systemv2, 0.01f))
						{
							info.SetValue(currentComponent, (Vector2) systemv2);
						}
					}
					else if (fieldOrPropertyType == typeof(AudioClip))
					{
						AudioClip audioClip = (AudioClip) info.GetValue(currentComponent);
						if (audioClip == null)
						{
							audioClip = new AudioClip();
							info.SetValue(currentComponent, audioClip);
						}

						string clipName = Path.GetFileName(audioClip?.path);

						bool clicked = ImGui.Button(clipName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));


						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_AUDIOCLIP", ImGuiDragDropFlags.None);
							string fileName = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && fileName.Length > 0)
							{
								fileName = Path.GetRelativePath("Assets", fileName);

								audioClip.path = fileName;
								info.SetValue(currentComponent, audioClip);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (fieldOrPropertyType == typeof(List<GameObject>))
					{
						List<GameObject> listOfGameObjects = (List<GameObject>) info.GetValue(currentComponent);
						if (ImGui.CollapsingHeader("List<GameObject>", ImGuiTreeNodeFlags.DefaultOpen))
						{
							for (int j = 0; j < listOfGameObjects.Count; j++)
							{
								ImGui.PushStyleColor(ImGuiCol.TextSelectedBg, Color.Aqua.ToVector4());
								PushNextID();
								bool xClicked = ImGui.Button("x", new System.Numerics.Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()));

								if (xClicked)
								{
									listOfGameObjects.RemoveAt(j);
									info.SetValue(currentComponent, listOfGameObjects);
									continue;
								}

								ImGui.SameLine();

								bool selectableClicked = ImGui.Selectable(listOfGameObjects[j].name);
								if (selectableClicked)
								{
									EditorWindow_Hierarchy.I.SelectGameObject(listOfGameObjects[j].id);
									return;
								}

								if (ImGui.BeginDragDropTarget())
								{
									ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);

									string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
									var x = ImGui.GetDragDropPayload();
									if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
									{
										GameObject foundGO = Scene.I.GetGameObject(int.Parse(payload));
										listOfGameObjects[j] = foundGO;
										info.SetValue(currentComponent, listOfGameObjects);
									}

									ImGui.EndDragDropTarget();
								}

								ImGui.PopStyleColor();
							}
						}
					}
					else if (fieldOrPropertyType == typeof(Action))
					{
						Action action = (Action) info.GetValue(currentComponent);
						ImGui.PushStyleColor(ImGuiCol.Text, Color.Linen.ToVector4());
						if (ImGui.Button($"> {info.Name} <", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight())))
						{
							action?.Invoke();
						}
						ImGui.PopStyleColor(1);
					}
					else if (fieldOrPropertyType == typeof(Texture) && currentComponent is SpriteRenderer)
					{
						string textureName = Path.GetFileName((currentComponent as SpriteRenderer).texture.path);

						bool clicked = ImGui.Button(textureName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						//ImiGui.Text(textureName);
						if (clicked)
						{
							EditorWindow_Browser.I.GoToFile((currentComponent as SpriteRenderer).texture.path);
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_TEXTURE", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
							{
								payload = Path.GetRelativePath("Assets", payload);

								textureName = payload;

								(currentComponent as SpriteRenderer).LoadTexture(textureName);
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (fieldOrPropertyType == typeof(Material))
					{
						string materialPath = Path.GetFileName((currentComponent as Renderer).material.path);

						materialPath = materialPath ?? "";
						bool clicked = ImGui.Button(materialPath, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						if (clicked)
						{
							EditorWindow_Browser.I.GoToFile(materialPath);
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("CONTENT_BROWSER_MATERIAL", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
							{
								payload = payload;
								string materialName = Path.GetFileName(payload);
								Material draggedMaterial = MaterialAssetManager.LoadMaterial(payload);
								if (draggedMaterial.shader == null)
								{
									Debug.Log("No Shader attached to material.");
								}
								else
								{
									(currentComponent as Renderer).material = draggedMaterial;
								}
								// load new material
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (fieldOrPropertyType == typeof(GameObject))
					{
						GameObject goObject = info.GetValue(currentComponent) as GameObject;
						string fieldGoName = goObject?.name ?? "";
						bool clicked = ImGui.Button(fieldGoName, new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()));
						if (clicked && goObject != null)
						{
							EditorWindow_Hierarchy.I.SelectGameObject(goObject.id);
							return;
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("PREFAB_PATH", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII().Replace("\0", string.Empty);
							if (dataType == "PREFAB_PATH")
							{
								if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
								{
									GameObject loadedGO = Serializer.I.LoadPrefab(payload.ToString(), inBackground: true);
									info.SetValue(currentComponent, loadedGO);
								}
							}

							ImGui.EndDragDropTarget();
						}

						if (ImGui.BeginDragDropTarget())
						{
							ImGui.AcceptDragDropPayload("GAMEOBJECT", ImGuiDragDropFlags.None);
							string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
							string dataType = ImGui.GetDragDropPayload().DataType.GetStringASCII().Replace("\0", string.Empty);

							if (dataType == "GAMEOBJECT")
							{
								//	string payload = Marshal.PtrToStringAnsi(ImGui.GetDragDropPayload().Data);
								if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && payload.Length > 0)
								{
									GameObject foundGO = Scene.I.GetGameObject(int.Parse(payload));
									info.SetValue(currentComponent, foundGO);
								}
							}

							ImGui.EndDragDropTarget();
						}
					}
					else if (fieldOrPropertyType == typeof(Color))
					{
						System.Numerics.Vector4 fieldValue = ((Color) info.GetValue(currentComponent)).ToVector4();

						if (ImGui.ColorEdit4("", ref fieldValue))
						{
							info.SetValue(currentComponent, fieldValue.ToColor());
						}
					}
					else if (fieldOrPropertyType == typeof(bool))
					{
						ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.GetContentRegionAvail().X / 2);

						bool fieldValue = (bool) info.GetValue(currentComponent);

						if (ImGui.Checkbox("", ref fieldValue))
						{
							info.SetValue(currentComponent, fieldValue);
						}
					}
					else if (fieldOrPropertyType == typeof(float))
					{
						float fieldValue = (float) info.GetValue(currentComponent);

						SliderF sliderAttrib = null;
						var a = fieldOrPropertyType.CustomAttributes.ToList();
						for (int i = 0; i < info.CustomAttributes.Count(); i++)
						{
							if (info.CustomAttributes.ElementAtOrDefault(i).AttributeType == typeof(SliderF))
							{
								FieldInfo fieldType = currentComponent.GetType().GetField(info.Name);
								sliderAttrib = fieldType.GetCustomAttribute<SliderF>();
							}
						}

						if (sliderAttrib != null)
						{
							if (ImGui.SliderFloat("", ref fieldValue, sliderAttrib.minValue, sliderAttrib.maxValue))
							{
								info.SetValue(currentComponent, fieldValue);
							}
						}
						else
						{
							if (ImGui.DragFloat("", ref fieldValue, 0.01f, float.NegativeInfinity, float.PositiveInfinity, "%.05f"))
							{
								info.SetValue(currentComponent, fieldValue);
							}
						}
					}
					else if (fieldOrPropertyType == typeof(int))
					{
						int fieldValue = (int) info.GetValue(currentComponent);


						if (ImGui.DragInt("", ref fieldValue))
						{
							info.SetValue(currentComponent, fieldValue);
						}
					}
					else if (fieldOrPropertyType == typeof(string))
					{
						string fieldValue = info.GetValue(currentComponent).ToString();

						if (ImGui.InputText("", ref fieldValue, 100))
						{
							info.SetValue(currentComponent, fieldValue);
						}
					}
					//ImGui.PopID();
				}

				//PropertyInfo[] properties = selectedGameObject.Components[i].GetType ().GetProperties ();
				//for (int j = 0; j < properties.Length; j++)
				//{
				//	PushNextID ();
				//	for (int k = 0; k < properties[j].CustomAttributes.Count (); k++)
				//	{
				//		if (properties[j].CustomAttributes.ElementAtOrDefault (k).AttributeType != typeof (ShowInEditor))
				//		{
				//			continue;
				//		}
				//		ImGui.Text (properties[j].Name);
				//		ImGui.SameLine ();
				//		//ImGui.Text (fieldInfo[j].GetValue (selectedGameObject.Components[i]).ToString ());

				//		//if (properties[j].PropertyType == typeof (float))
				//		//{
				//		//	float fl = (float) properties[j].GetValue (selectedGameObject.Components[i]);
				//		//	if (ImGui.DragFloat ("", ref fl, 0.01f, 0, 1))
				//		//	{
				//		//		properties[j].SetValue (selectedGameObject.Components[i], fl);
				//		//	}
				//		//}
				//		if (properties[j].PropertyType == typeof (Vector3))
				//		{
				//			Vector3 fl = (Vector3) properties[j].GetValue (selectedGameObject.Components[i]);
				//			if (ImGui.DragFloat3 ("", ref fl, 0.01f))
				//			{
				//				//properties[j].SetValue (selectedGameObject.Components[i], fl);
				//			}
				//		}
				//	}
				//	ImGui.PopID ();
				//}
			}
		}

		bool justOpened = false;
		if (ImGui.Button("+"))
		{
			ImGui.OpenPopup("AddComponentPopup");
			justOpened = true;
		}

		if (ImGui.BeginPopupContextWindow("AddComponentPopup"))
		{
			if (justOpened)
			{
				ImGui.SetKeyboardFocusHere(0);
			}

			bool enterPressed = ImGui.InputText("", ref addComponentPopupText, 100, ImGuiInputTextFlags.EnterReturnsTrue);


			if (addComponentPopupText.Length > 0)
			{
				List<Type> componentTypes = typeof(Component).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();

				for (int i = 0; i < componentTypes.Count; i++)
					if (componentTypes[i].Name.ToLower().Contains(addComponentPopupText.ToLower()))
					{
						if (ImGui.Button(componentTypes[i].Name) || enterPressed)
						{
							selectedGameObject.AddComponent(componentTypes[i]);
							ImGui.CloseCurrentPopup();
							break;
						}
					}
			}
			else
			{
				List<Type> componentTypes = typeof(Component).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract).ToList();

				for (int i = 0; i < componentTypes.Count; i++)
					if (ImGui.Button(componentTypes[i].Name))
					{
						selectedGameObject.AddComponent(componentTypes[i]);
						ImGui.CloseCurrentPopup();
					}
			}

			ImGui.EndPopup();
		}
	}
}