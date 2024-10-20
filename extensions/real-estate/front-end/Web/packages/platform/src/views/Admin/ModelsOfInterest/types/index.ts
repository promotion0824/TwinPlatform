/**
 * formMode is null when were in the initial state in /admin/modelsOfInterest page where the form is not opened.
 * formMode is 'add' when we click on the "Add Model of Interest" button.
 * formMode is 'edit' when we click on the edit button.
 * formMode is used to conditionally render specific texts in the form.
 */
export type FormMode = 'add' | 'edit' | null

type ModelOfInterest = {
  modelId: string
  name: string
  color: string
  text: string
}
export type PartialModelOfInterest = Partial<ModelOfInterest>
export type ExistingModelOfInterest = ModelOfInterest & { id: string }
