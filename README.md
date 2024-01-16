## DEWESoft OnGetData Flooding Bug
The following video showcases the bug discussed in [this DEWESoft developer forum](https://developer.dewesoft.com/questions/flooding-the-iappongetdata-event-overwhelms-dewesoft-ui) and how to replicate it.

Look for Media/Showcase.mp4 and view it directly if the video tag does not work.

<video controls>
  <source src="Media/Showcase.mp4" type="video/mp4">
</video>

Developer's Note: If the user never interacts with DEWESoft while data is being fetched, the application will remain stable. Any potential issues are likely related to the UI thread of DEWESoft.
